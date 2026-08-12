using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.LLM;
using RimTalk.ExpandActions.Parsing;
using RimTalk.ExpandActions.Util;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RimTalk.ExpandActions.Integration;

/// <summary>
/// One-shot, marker-gated integration probe for the disposable Stage 6 save.
/// It is inert during ordinary gameplay and never logs prompts or provider output.
/// </summary>
public sealed class Stage6RuntimeProbe : GameComponent
{
	private const string SaveName = "Stage6_AI_E2E_Disposable";
	private const string MarkerName = "Stage6.ExpandActionsE2E.request";
	private const string Command = "Підійми з підлоги 2000 срібла та поклади собі в кишеню.";

	private bool started;
	private bool waitingForCompletion;
	private int actorId;
	private int beforeSilver;
	private int deadlineTick;
	private string correlationId;

	public Stage6RuntimeProbe(Game game)
	{
	}

	public override void GameComponentTick()
	{
		if (!started && Find.TickManager.TicksGame % 60 == 0)
		{
			TryStart();
		}
		if (waitingForCompletion && Find.TickManager.TicksGame % 30 == 0)
		{
			CheckCompletion();
		}
	}

	private void TryStart()
	{
		string marker = Path.Combine(GenFilePaths.SaveDataFolderPath, MarkerName);
		if (!File.Exists(marker) || !string.Equals(Find.World?.info?.FileNameNoExtension, SaveName, StringComparison.Ordinal))
		{
			return;
		}
		started = true;
		File.Delete(marker);
		Map map = Find.CurrentMap;
		Pawn actor = map?.mapPawns.FreeColonistsSpawned.FirstOrDefault(p => !p.Dead && !p.Downed);
		if (actor == null)
		{
			EALogger.Error("[EA_STAGE6] state=FAILED reason=NoEligibleColonist");
			return;
		}

		actorId = actor.thingIDNumber;
		beforeSilver = CountInventorySilver(actor);
		SpawnSilver(map, actor, 2000);
		correlationId = "stage6-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
		EALogger.Info($"[EA_STAGE6] conv={correlationId} state=PARSED_PENDING actor={actorId} before={beforeSilver}");
		_ = ResolveAndExecuteAsync(map, actor);
	}

	private async System.Threading.Tasks.Task ResolveAndExecuteAsync(Map map, Pawn actor)
	{
		ToolcallResponse response = await SecondaryLLMCaller.ConvertBehaviorsAsync(new List<string> { Command }, actor.LabelShort);
		LongEventHandler.ExecuteWhenFinished(() =>
		{
			ActionCall action = response?.Actions?.FirstOrDefault();
			if (action == null)
			{
				EALogger.Error($"[EA_STAGE6] conv={correlationId} state=UNSUPPORTED reason=NoCanonicalAction");
				return;
			}
			action.Actor = actor.LabelShort;
			ExecutionResult result = ActionExecutor.ExecuteSingle(correlationId, action, map);
			EALogger.Info($"[EA_STAGE6] conv={correlationId} state=RESOLVED action={action.Id} accepted={result.Success} lifecycle={result.State}");
			if (!result.Success)
			{
				EALogger.Error($"[EA_STAGE6] conv={correlationId} state=FAILED action={action.Id} code={result.ErrorCode}");
				return;
			}
			waitingForCompletion = true;
			deadlineTick = Find.TickManager.TicksGame + 6000;
		});
	}

	private void CheckCompletion()
	{
		Pawn actor = Find.CurrentMap?.mapPawns.AllPawnsSpawned.FirstOrDefault(p => p.thingIDNumber == actorId);
		if (actor == null)
		{
			waitingForCompletion = false;
			EALogger.Error($"[EA_STAGE6] conv={correlationId} state=FAILED reason=ActorMissing");
			return;
		}
		int after = CountInventorySilver(actor);
		if (after - beforeSilver >= 2000)
		{
			waitingForCompletion = false;
			EALogger.Info($"[EA_STAGE6] conv={correlationId} state=COMPLETED action=take_inventory delta={after - beforeSilver}");
		}
		else if (Find.TickManager.TicksGame >= deadlineTick)
		{
			waitingForCompletion = false;
			EALogger.Error($"[EA_STAGE6] conv={correlationId} state=FAILED action=take_inventory reason=InventoryDelta delta={after - beforeSilver}");
		}
	}

	private static int CountInventorySilver(Pawn pawn)
	{
		return pawn.inventory?.innerContainer.Where(t => t.def == ThingDefOf.Silver).Sum(t => t.stackCount) ?? 0;
	}

	private static void SpawnSilver(Map map, Pawn actor, int count)
	{
		IntVec3 cell = CellFinder.RandomClosewalkCellNear(actor.Position, map, 3);
		while (count > 0)
		{
			Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
			silver.stackCount = Math.Min(count, ThingDefOf.Silver.stackLimit);
			GenSpawn.Spawn(silver, cell, map);
			count -= silver.stackCount;
		}
	}
}
