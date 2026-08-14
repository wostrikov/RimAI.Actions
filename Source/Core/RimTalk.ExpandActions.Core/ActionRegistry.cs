using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimTalk.ExpandActions.Actions.Animal;
using RimTalk.ExpandActions.Actions.Combat;
using RimTalk.ExpandActions.Actions.Facility;
using RimTalk.ExpandActions.Actions.Funeral;
using RimTalk.ExpandActions.Actions.Item;
using RimTalk.ExpandActions.Actions.Job;
using RimTalk.ExpandActions.Actions.Medical;
using RimTalk.ExpandActions.Actions.Movement;
using RimTalk.ExpandActions.Actions.Prisoner;
using RimTalk.ExpandActions.Actions.Production;
using RimTalk.ExpandActions.Actions.Recreation;
using RimTalk.ExpandActions.Actions.Social;
using RimTalk.ExpandActions.Integration;
using RimTalk.ExpandActions.Mod;
using RimTalk.ExpandActions.Util;

namespace RimTalk.ExpandActions.Core;

public static class ActionRegistry
{
	private static readonly Dictionary<string, ActionDefinition> _actions = new Dictionary<string, ActionDefinition>();

	private static bool _initialized = false;

	public static void Initialize()
	{
		if (!_initialized)
		{
			RegisterAllActions();
			_initialized = true;
			EALogger.Info($"ActionRegistry initialized with {_actions.Count} built-in actions");
		}
	}

	public static void Register(ActionDefinition action)
	{
		if (_actions.ContainsKey(action.Id))
		{
			EALogger.Warn("Action '" + action.Id + "' is already registered, overwriting");
		}
		_actions[action.Id] = action;
	}

	public static ActionDefinition GetById(string id)
	{
		if (!_actions.TryGetValue(id, out var value))
		{
			return null;
		}
		return value;
	}

	public static IEnumerable<ActionDefinition> GetAll()
	{
		return _actions.Values;
	}

	public static IEnumerable<ActionDefinition> GetByCategory(ActionCategory category)
	{
		return _actions.Values.Where((ActionDefinition a) => a.Category == category);
	}

	public static IEnumerable<ActionDefinition> GetEnabledActions()
	{
		EASettings settings = EAModMain.Settings;
		return _actions.Values.Where((ActionDefinition a) => settings.IsActionEnabled(a.Id));
	}

	public static string GetEnabledActionsPrompt()
	{
		EASettings settings = EAModMain.Settings;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (ActionDefinition enabledAction in GetEnabledActions())
		{
			string promptDesc = settings.GetPromptDesc(enabledAction.Id, enabledAction.DefaultPromptDesc);
			string text = string.Join(", ", enabledAction.RequiredParams);
			if (enabledAction.OptionalParams.Count > 0)
			{
				text = text + ((text.Length > 0) ? ", " : "") + "[" + string.Join(", ", enabledAction.OptionalParams) + "]";
			}
			stringBuilder.AppendLine("- " + enabledAction.Id + "(" + text + "): " + promptDesc);
		}
		return stringBuilder.ToString();
	}

	public static bool IsActionValid(string id)
	{
		if (_actions.ContainsKey(id))
		{
			return EAModMain.Settings.IsActionEnabled(id);
		}
		return false;
	}

	private static void RegisterAllActions()
	{
		Register(new ActionDefinition
		{
			Id = "move_to",
			Category = ActionCategory.Movement,
			DisplayName = "EA_Action_move_to",
			Description = "EA_Action_move_to_Desc",
			DefaultPromptDesc = "Move to target location",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "target", "cell" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "move", "go", "walk", "走", "移动", "前往", "过去" }
		});
		Register(new ActionDefinition
		{
			Id = "stop",
			Category = ActionCategory.Movement,
			DisplayName = "EA_Action_stop",
			Description = "EA_Action_stop_Desc",
			DefaultPromptDesc = "Immediately stop current action",
			RequiredParams = new List<string> { "actor" },
			RiskLevel = RiskLevel.Low,
			DefaultEnabled = false,
			Handler = null,
			Keywords = new List<string> { "stop", "halt", "cease", "停", "停止", "停下" }
		});
		Register(new ActionDefinition
		{
			Id = "wait",
			Category = ActionCategory.Movement,
			DisplayName = "EA_Action_wait",
			Description = "EA_Action_wait_Desc",
			DefaultPromptDesc = "Wait at current location for specified ticks",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "args.ticks" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "wait", "stay", "stand", "等", "等待", "待命", "站立", "站" }
		});
		Register(new ActionDefinition
		{
			Id = "follow",
			Category = ActionCategory.Movement,
			DisplayName = "EA_Action_follow",
			Description = "EA_Action_follow_Desc",
			DefaultPromptDesc = "Follow target pawn",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "follow", "跟随", "跟着", "跟上" }
		});
		Register(new ActionDefinition
		{
			Id = "flee",
			Category = ActionCategory.Movement,
			DisplayName = "EA_Action_flee",
			Description = "EA_Action_flee_Desc",
			DefaultPromptDesc = "Flee from danger or target",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "target" },
			RiskLevel = RiskLevel.Low,
			Handler = new FleeHandler(),
			Keywords = new List<string> { "flee", "run", "escape", "逃", "逃跑", "逃离" }
		});
		Register(new ActionDefinition
		{
			Id = "draft_set",
			Category = ActionCategory.Combat,
			DisplayName = "EA_Action_draft_set",
			Description = "EA_Action_draft_set_Desc",
			DefaultPromptDesc = "Set pawn draft status (args.drafted: true/false)",
			RequiredParams = new List<string> { "actor", "args.drafted" },
			RiskLevel = RiskLevel.Medium,
			Handler = new DraftSetHandler(),
			Keywords = new List<string> { "draft", "undraft", "征召", "解除征召", "备战" }
		});
		Register(new ActionDefinition
		{
			Id = "attack_melee",
			Category = ActionCategory.Combat,
			DisplayName = "EA_Action_attack_melee",
			Description = "EA_Action_attack_melee_Desc",
			DefaultPromptDesc = "Attack target with melee weapon",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = new AttackMeleeHandler(),
			Keywords = new List<string> { "attack", "melee", "hit", "punch", "近战", "攻击", "打" }
		});
		Register(new ActionDefinition
		{
			Id = "attack_ranged",
			Category = ActionCategory.Combat,
			DisplayName = "EA_Action_attack_ranged",
			Description = "EA_Action_attack_ranged_Desc",
			DefaultPromptDesc = "Attack target with ranged weapon",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = new AttackRangedHandler(),
			Keywords = new List<string> { "shoot", "ranged", "fire", "射击", "远程", "开枪", "射" }
		});
		Register(new ActionDefinition
		{
			Id = "arrest",
			Category = ActionCategory.Combat,
			DisplayName = "EA_Action_arrest",
			Description = "EA_Action_arrest_Desc",
			DefaultPromptDesc = "Attempt to arrest target pawn",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = new ArrestHandler(),
			Keywords = new List<string> { "arrest", "逮捕", "抓捕" }
		});
		Register(new ActionDefinition
		{
			Id = "drop_weapon",
			Category = ActionCategory.Combat,
			DisplayName = "EA_Action_drop_weapon",
			Description = "EA_Action_drop_weapon_Desc",
			DefaultPromptDesc = "Drop currently equipped weapon",
			RequiredParams = new List<string> { "actor" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "drop weapon", "disarm", "丢弃武器", "放下武器" }
		});
		Register(new ActionDefinition
		{
			Id = "mental_break_start",
			Category = ActionCategory.Combat,
			DisplayName = "EA_Action_mental_break_start",
			Description = "EA_Action_mental_break_start_Desc",
			DefaultPromptDesc = "Trigger mental state (args.state: MentalStateDef name)",
			RequiredParams = new List<string> { "actor", "args.state" },
			RiskLevel = RiskLevel.High,
			DefaultEnabled = false,
			Handler = new MentalBreakStartHandler(),
			Keywords = new List<string> { "mental", "break", "berserk", "精神崩溃", "发狂" }
		});
		Register(new ActionDefinition
		{
			Id = "rescue",
			Category = ActionCategory.Medical,
			DisplayName = "EA_Action_rescue",
			Description = "EA_Action_rescue_Desc",
			DefaultPromptDesc = "Rescue downed pawn to bed",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "rescue", "save", "carry", "救援", "营救", "搬运" }
		});
		Register(new ActionDefinition
		{
			Id = "tend",
			Category = ActionCategory.Medical,
			DisplayName = "EA_Action_tend",
			Description = "EA_Action_tend_Desc",
			DefaultPromptDesc = "Tend to injured pawn",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "tend", "heal", "treat", "治疗", "包扎", "医治" }
		});
		Register(new ActionDefinition
		{
			Id = "feed_patient",
			Category = ActionCategory.Medical,
			DisplayName = "EA_Action_feed_patient",
			Description = "EA_Action_feed_patient_Desc",
			DefaultPromptDesc = "Feed bedridden patient",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "feed", "food", "patient", "喂食", "喂饭", "病人" }
		});
		Register(new ActionDefinition
		{
			Id = "force_sleep",
			Category = ActionCategory.Medical,
			DisplayName = "EA_Action_force_sleep",
			Description = "EA_Action_force_sleep_Desc",
			DefaultPromptDesc = "Force pawn to rest",
			RequiredParams = new List<string> { "actor" },
			RiskLevel = RiskLevel.Low,
			Handler = new ForceSleepHandler(),
			Keywords = new List<string> { "sleep", "nap", "rest", "睡觉", "睡", "躺下", "休息", "歇" }
		});
		Register(new ActionDefinition
		{
			Id = "recruit",
			Category = ActionCategory.Social,
			DisplayName = "EA_Action_recruit",
			Description = "EA_Action_recruit_Desc",
			DefaultPromptDesc = "Attempt to recruit prisoner",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = new RecruitHandler(),
			Keywords = new List<string> { "recruit", "persuade", "招募", "说服" }
		});
		Register(new ActionDefinition
		{
			Id = "romance_set",
			Category = ActionCategory.Social,
			DisplayName = "EA_Action_romance_set",
			Description = "EA_Action_romance_set_Desc",
			DefaultPromptDesc = "Set romantic relationship (args.mode: new_lover/breakup)",
			RequiredParams = new List<string> { "actor", "target", "args.mode" },
			RiskLevel = RiskLevel.High,
			DefaultEnabled = false,
			Handler = new RomanceSetHandler(),
			Keywords = new List<string> { "romance", "lover", "breakup", "恋爱", "分手", "告白" }
		});
		Register(new ActionDefinition
		{
			Id = "thought_add",
			Category = ActionCategory.Social,
			DisplayName = "EA_Action_thought_add",
			Description = "EA_Action_thought_add_Desc",
			DefaultPromptDesc = "Add memory thought (args.thought: ThoughtDef name)",
			RequiredParams = new List<string> { "actor", "args.thought" },
			RiskLevel = RiskLevel.Medium,
			Handler = new ThoughtAddHandler(),
			Keywords = new List<string> { "thought", "memory", "mood", "思绪", "心情", "记忆" }
		});
		Register(new ActionDefinition
		{
			Id = "relation_set",
			Category = ActionCategory.Social,
			DisplayName = "EA_Action_relation_set",
			Description = "EA_Action_relation_set_Desc",
			DefaultPromptDesc = "Modify pawn relationship (args.relation, args.mode: add/remove)",
			RequiredParams = new List<string> { "actor", "target", "args.relation", "args.mode" },
			RiskLevel = RiskLevel.High,
			DefaultEnabled = false,
			Handler = new RelationSetHandler(),
			Keywords = new List<string> { "relation", "friend", "enemy", "关系", "朋友", "敌人" }
		});
		Register(new ActionDefinition
		{
			Id = "give_inspiration",
			Category = ActionCategory.Social,
			DisplayName = "EA_Action_give_inspiration",
			Description = "EA_Action_give_inspiration_Desc",
			DefaultPromptDesc = "Grant inspiration (args.type: InspirationDef name)",
			RequiredParams = new List<string> { "actor", "args.type" },
			RiskLevel = RiskLevel.Medium,
			Handler = new GiveInspirationHandler(),
			Keywords = new List<string> { "inspiration", "inspire", "灵感", "激励" }
		});
		Register(new ActionDefinition
		{
			Id = "visit_sick",
			Category = ActionCategory.Social,
			DisplayName = "EA_Action_visit_sick",
			Description = "EA_Action_visit_sick_Desc",
			DefaultPromptDesc = "Visit sick or bedridden pawn",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = new VisitSickHandler(),
			Keywords = new List<string> { "visit", "sick", "ill", "探病", "看望", "探望" }
		});
		Register(new ActionDefinition
		{
			Id = "social_fight",
			Category = ActionCategory.Social,
			DisplayName = "EA_Action_social_fight",
			Description = "EA_Action_social_fight_Desc",
			DefaultPromptDesc = "Start a social fight with target",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			DefaultEnabled = false,
			Handler = new SocialFightHandler(),
			Keywords = new List<string> { "fight", "brawl", "social fight", "打架", "吵架", "斗殴" }
		});
		Register(new ActionDefinition
		{
			Id = "lovin",
			Category = ActionCategory.Social,
			DisplayName = "EA_Action_lovin",
			Description = "EA_Action_lovin_Desc",
			DefaultPromptDesc = "Lovin' with partner in bed",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = new LovinHandler(),
			Keywords = new List<string> { "lovin", "亲热", "恩爱" }
		});
		Register(new ActionDefinition
		{
			Id = "insult",
			Category = ActionCategory.Social,
			DisplayName = "EA_Action_insult",
			Description = "EA_Action_insult_Desc",
			DefaultPromptDesc = "Insult target pawn",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			DefaultEnabled = false,
			Handler = new InsultHandler(),
			Keywords = new List<string> { "insult", "mock", "侮辱", "嘲笑", "骂" }
		});
		Register(new ActionDefinition
		{
			Id = "marry",
			Category = ActionCategory.Social,
			DisplayName = "EA_Action_marry",
			Description = "EA_Action_marry_Desc",
			DefaultPromptDesc = "Marry target pawn (ceremony)",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.High,
			DefaultEnabled = false,
			Handler = new MarryHandler(),
			Keywords = new List<string> { "marry", "wedding", "marriage", "结婚", "婚礼" }
		});
		Register(new ActionDefinition
		{
			Id = "spectate",
			Category = ActionCategory.Social,
			DisplayName = "EA_Action_spectate",
			Description = "EA_Action_spectate_Desc",
			DefaultPromptDesc = "Watch spectacle or event",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = new SpectateHandler(),
			Keywords = new List<string> { "spectate", "watch", "observe", "观看", "观赏" }
		});
		Register(new ActionDefinition
		{
			Id = "take_inventory",
			Category = ActionCategory.Item,
			DisplayName = "EA_Action_take_inventory",
			Description = "EA_Action_take_inventory_Desc",
			DefaultPromptDesc = "Pick up an item from the ground into personal inventory (thing; args.quantity, partial stacks are allowed)",
			RequiredParams = new List<string> { "actor", "thing" },
			OptionalParams = new List<string> { "target", "args.quantity" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "take inventory", "pick up", "pickup", "put in pocket", "візьми", "підійми", "підбери", "у кишеню", "інвентар", "捡起", "放进背包" }
		});
		Register(new ActionDefinition
		{
			Id = "drop_item",
			Category = ActionCategory.Item,
			DisplayName = "EA_Action_drop_item",
			Description = "EA_Action_drop_item_Desc",
			DefaultPromptDesc = "Drop held item",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "thing" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "drop", "丢弃", "放下", "丢" }
		});
		Register(new ActionDefinition
		{
			Id = "give_item",
			Category = ActionCategory.Item,
			DisplayName = "EA_Action_give_item",
			Description = "EA_Action_give_item_Desc",
			DefaultPromptDesc = "Give item to target pawn",
			RequiredParams = new List<string> { "actor", "target" },
			OptionalParams = new List<string> { "thing" },
			RiskLevel = RiskLevel.Medium,
			Handler = new GiveItemHandler(),
			Keywords = new List<string> { "give", "hand", "赠送", "给", "递给" }
		});
		Register(new ActionDefinition
		{
			Id = "equip",
			Category = ActionCategory.Item,
			DisplayName = "EA_Action_equip",
			Description = "EA_Action_equip_Desc",
			DefaultPromptDesc = "Equip weapon or apparel",
			RequiredParams = new List<string> { "actor", "thing" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "equip", "wear", "装备", "穿上", "武装" }
		});
		Register(new ActionDefinition
		{
			Id = "force_eat",
			Category = ActionCategory.Item,
			DisplayName = "EA_Action_force_eat",
			Description = "EA_Action_force_eat_Desc",
			DefaultPromptDesc = "Force pawn to eat food",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "thing" },
			RiskLevel = RiskLevel.Low,
			Handler = new ForceEatHandler(),
			Keywords = new List<string> { "eat", "food", "吃", "进食", "吃饭", "用餐" }
		});
		Register(new ActionDefinition
		{
			Id = "job_start",
			Category = ActionCategory.Job,
			DisplayName = "EA_Action_job_start",
			Description = "EA_Action_job_start_Desc",
			DefaultPromptDesc = "Start a specific job (job: JobDef name)",
			RequiredParams = new List<string> { "actor", "job" },
			OptionalParams = new List<string> { "target", "cell" },
			RiskLevel = RiskLevel.Medium,
			Handler = new JobStartHandler(),
			Keywords = new List<string> { "job", "work", "task", "任务", "工作" }
		});
		Register(new ActionDefinition
		{
			Id = "job_queue_front",
			Category = ActionCategory.Job,
			DisplayName = "EA_Action_job_queue_front",
			Description = "EA_Action_job_queue_front_Desc",
			DefaultPromptDesc = "Queue job at front (job: JobDef name)",
			RequiredParams = new List<string> { "actor", "job" },
			OptionalParams = new List<string> { "target", "cell" },
			RiskLevel = RiskLevel.Medium,
			Handler = new JobQueueFrontHandler(),
			Keywords = new List<string> { "queue", "排队", "队列" }
		});
		Register(new ActionDefinition
		{
			Id = "job_end",
			Category = ActionCategory.Job,
			DisplayName = "EA_Action_job_end",
			Description = "EA_Action_job_end_Desc",
			DefaultPromptDesc = "End current job",
			RequiredParams = new List<string> { "actor" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "end job", "cancel", "结束任务", "取消" }
		});
		Register(new ActionDefinition
		{
			Id = "capture",
			Category = ActionCategory.Prisoner,
			DisplayName = "EA_Action_capture",
			Description = "EA_Action_capture_Desc",
			DefaultPromptDesc = "Capture downed pawn as prisoner",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = new CaptureHandler(),
			Keywords = new List<string> { "capture", "prisoner", "抓住", "俘虏", "抓获" }
		});
		Register(new ActionDefinition
		{
			Id = "release_prisoner",
			Category = ActionCategory.Prisoner,
			DisplayName = "EA_Action_release_prisoner",
			Description = "EA_Action_release_prisoner_Desc",
			DefaultPromptDesc = "Release prisoner",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = new ReleasePrisonerHandler(),
			Keywords = new List<string> { "release", "free", "释放", "放走" }
		});
		Register(new ActionDefinition
		{
			Id = "recruit_prisoner",
			Category = ActionCategory.Prisoner,
			DisplayName = "EA_Action_recruit_prisoner",
			Description = "EA_Action_recruit_prisoner_Desc",
			DefaultPromptDesc = "Attempt to recruit prisoner",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = new RecruitPrisonerHandler(),
			Keywords = new List<string> { "recruit prisoner", "招募囚犯", "说服囚犯" }
		});
		Register(new ActionDefinition
		{
			Id = "execute",
			Category = ActionCategory.Prisoner,
			DisplayName = "EA_Action_execute",
			Description = "EA_Action_execute_Desc",
			DefaultPromptDesc = "Execute prisoner",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.High,
			DefaultEnabled = false,
			Handler = new ExecuteHandler(),
			Keywords = new List<string> { "execute", "kill prisoner", "处决", "处刑" }
		});
		Register(new ActionDefinition
		{
			Id = "escort_to_bed",
			Category = ActionCategory.Prisoner,
			DisplayName = "EA_Action_escort_to_bed",
			Description = "EA_Action_escort_to_bed_Desc",
			DefaultPromptDesc = "Escort prisoner to bed",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = new EscortToBedHandler(),
			Keywords = new List<string> { "escort", "bed", "押送", "带回" }
		});
		Register(new ActionDefinition
		{
			Id = "strip",
			Category = ActionCategory.Prisoner,
			DisplayName = "EA_Action_strip",
			Description = "EA_Action_strip_Desc",
			DefaultPromptDesc = "Strip apparel from target",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = new StripHandler(),
			Keywords = new List<string> { "strip", "undress", "剥取", "脱衣" }
		});
		Register(new ActionDefinition
		{
			Id = "tame",
			Category = ActionCategory.Animal,
			DisplayName = "EA_Action_tame",
			Description = "EA_Action_tame_Desc",
			DefaultPromptDesc = "Attempt to tame animal",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "tame", "domesticate", "驯服", "驯化" }
		});
		Register(new ActionDefinition
		{
			Id = "train",
			Category = ActionCategory.Animal,
			DisplayName = "EA_Action_train",
			Description = "EA_Action_train_Desc",
			DefaultPromptDesc = "Train animal",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "train", "训练", "教导" }
		});
		Register(new ActionDefinition
		{
			Id = "hunt",
			Category = ActionCategory.Animal,
			DisplayName = "EA_Action_hunt",
			Description = "EA_Action_hunt_Desc",
			DefaultPromptDesc = "Hunt animal",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = new HuntHandler(),
			Keywords = new List<string> { "hunt", "狩猎", "猎杀", "打猎" }
		});
		Register(new ActionDefinition
		{
			Id = "slaughter",
			Category = ActionCategory.Animal,
			DisplayName = "EA_Action_slaughter",
			Description = "EA_Action_slaughter_Desc",
			DefaultPromptDesc = "Slaughter animal",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = new SlaughterHandler(),
			Keywords = new List<string> { "slaughter", "butcher", "屠宰", "宰杀" }
		});
		Register(new ActionDefinition
		{
			Id = "milk",
			Category = ActionCategory.Animal,
			DisplayName = "EA_Action_milk",
			Description = "EA_Action_milk_Desc",
			DefaultPromptDesc = "Milk animal",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "milk", "挤奶" }
		});
		Register(new ActionDefinition
		{
			Id = "shear",
			Category = ActionCategory.Animal,
			DisplayName = "EA_Action_shear",
			Description = "EA_Action_shear_Desc",
			DefaultPromptDesc = "Shear animal",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "shear", "wool", "剪毛" }
		});
		Register(new ActionDefinition
		{
			Id = "haul",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_haul",
			Description = "EA_Action_haul_Desc",
			DefaultPromptDesc = "Haul item to stockpile",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "haul", "carry", "搬运", "搬", "运输", "运", "扛" }
		});
		Register(new ActionDefinition
		{
			Id = "mine",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_mine",
			Description = "EA_Action_mine_Desc",
			DefaultPromptDesc = "Mine rock or mineral",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "mine", "dig", "采矿", "挖矿", "挖" }
		});
		Register(new ActionDefinition
		{
			Id = "cut_plant",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_cut_plant",
			Description = "EA_Action_cut_plant_Desc",
			DefaultPromptDesc = "Cut plant",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "cut", "chop", "plant", "砍", "砍伐", "植物" }
		});
		Register(new ActionDefinition
		{
			Id = "clean",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_clean",
			Description = "EA_Action_clean_Desc",
			DefaultPromptDesc = "Clean area",
			RequiredParams = new List<string> { "actor" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "clean", "sweep", "清洁", "打扫", "扫地" }
		});
		Register(new ActionDefinition
		{
			Id = "repair",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_repair",
			Description = "EA_Action_repair_Desc",
			DefaultPromptDesc = "Repair building or structure",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "repair", "fix", "修理", "修复" }
		});
		Register(new ActionDefinition
		{
			Id = "deconstruct",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_deconstruct",
			Description = "EA_Action_deconstruct_Desc",
			DefaultPromptDesc = "Deconstruct building",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = null,
			Keywords = new List<string> { "deconstruct", "demolish", "拆除", "拆" }
		});
		Register(new ActionDefinition
		{
			Id = "sow",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_sow",
			Description = "EA_Action_sow_Desc",
			DefaultPromptDesc = "Sow seeds in growing zone",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "target", "cell" },
			RiskLevel = RiskLevel.Low,
			Handler = new SowHandler(),
			Keywords = new List<string> { "sow", "plant", "seed", "播种", "种植" }
		});
		Register(new ActionDefinition
		{
			Id = "harvest",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_harvest",
			Description = "EA_Action_harvest_Desc",
			DefaultPromptDesc = "Harvest mature plant",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "harvest", "reap", "收割", "收获" }
		});
		Register(new ActionDefinition
		{
			Id = "research",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_research",
			Description = "EA_Action_research_Desc",
			DefaultPromptDesc = "Research at research bench",
			RequiredParams = new List<string> { "actor" },
			RiskLevel = RiskLevel.Low,
			Handler = new ResearchHandler(),
			Keywords = new List<string> { "research", "study", "研究", "科研" }
		});
		Register(new ActionDefinition
		{
			Id = "craft",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_craft",
			Description = "EA_Action_craft_Desc",
			DefaultPromptDesc = "Work at crafting bench",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "target" },
			RiskLevel = RiskLevel.Low,
			Handler = new CraftHandler(),
			Keywords = new List<string> { "craft", "make", "build", "制作", "制造", "打造" }
		});
		Register(new ActionDefinition
		{
			Id = "smooth_floor",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_smooth_floor",
			Description = "EA_Action_smooth_floor_Desc",
			DefaultPromptDesc = "Smooth rough floor",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "smooth", "floor", "抛光", "地面" }
		});
		Register(new ActionDefinition
		{
			Id = "build_roof",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_build_roof",
			Description = "EA_Action_build_roof_Desc",
			DefaultPromptDesc = "Build roof in area",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "target", "cell" },
			RiskLevel = RiskLevel.Low,
			Handler = new BuildRoofHandler(),
			Keywords = new List<string> { "roof", "cover", "屋顶", "建造屋顶" }
		});
		Register(new ActionDefinition
		{
			Id = "remove_roof",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_remove_roof",
			Description = "EA_Action_remove_roof_Desc",
			DefaultPromptDesc = "Remove roof from area",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "target", "cell" },
			RiskLevel = RiskLevel.Low,
			Handler = new RemoveRoofHandler(),
			Keywords = new List<string> { "remove roof", "拆除屋顶" }
		});
		Register(new ActionDefinition
		{
			Id = "uninstall",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_uninstall",
			Description = "EA_Action_uninstall_Desc",
			DefaultPromptDesc = "Uninstall building or furniture",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = null,
			Keywords = new List<string> { "uninstall", "remove", "卸载", "拆卸" }
		});
		Register(new ActionDefinition
		{
			Id = "fix_broken",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_fix_broken",
			Description = "EA_Action_fix_broken_Desc",
			DefaultPromptDesc = "Fix broken-down building",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "fix", "broken", "修复", "故障" }
		});
		Register(new ActionDefinition
		{
			Id = "refuel",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_refuel",
			Description = "EA_Action_refuel_Desc",
			DefaultPromptDesc = "Refuel building",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = new RefuelHandler(),
			Keywords = new List<string> { "refuel", "fuel", "加油", "燃料" }
		});
		Register(new ActionDefinition
		{
			Id = "flick",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_flick",
			Description = "EA_Action_flick_Desc",
			DefaultPromptDesc = "Toggle power switch on building",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "flick", "switch", "toggle", "开关", "切换" }
		});
		Register(new ActionDefinition
		{
			Id = "stargaze",
			Category = ActionCategory.Recreation,
			DisplayName = "EA_Action_stargaze",
			Description = "EA_Action_stargaze_Desc",
			DefaultPromptDesc = "Go stargazing outdoors",
			RequiredParams = new List<string> { "actor" },
			RiskLevel = RiskLevel.Low,
			Handler = new StargazeHandler(),
			Keywords = new List<string> { "stargaze", "stars", "sky", "观星", "看星星" }
		});
		Register(new ActionDefinition
		{
			Id = "go_for_walk",
			Category = ActionCategory.Recreation,
			DisplayName = "EA_Action_go_for_walk",
			Description = "EA_Action_go_for_walk_Desc",
			DefaultPromptDesc = "Take a leisurely walk",
			RequiredParams = new List<string> { "actor" },
			RiskLevel = RiskLevel.Low,
			Handler = new GoForWalkHandler(),
			Keywords = new List<string> { "walk", "stroll", "wander", "散步", "闲逛", "逛", "溜达" }
		});
		Register(new ActionDefinition
		{
			Id = "play_music",
			Category = ActionCategory.Recreation,
			DisplayName = "EA_Action_play_music",
			Description = "EA_Action_play_music_Desc",
			DefaultPromptDesc = "Play musical instrument",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "target" },
			RiskLevel = RiskLevel.Low,
			Handler = new PlayMusicHandler(),
			Keywords = new List<string> { "music", "play", "instrument", "音乐", "演奏", "乐器" }
		});
		Register(new ActionDefinition
		{
			Id = "use_drug",
			Category = ActionCategory.Recreation,
			DisplayName = "EA_Action_use_drug",
			Description = "EA_Action_use_drug_Desc",
			DefaultPromptDesc = "Use drug or consumable",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = new UseDrugHandler(),
			Keywords = new List<string> { "drug", "smoke", "drink", "药物", "毒品", "嗑药" }
		});
		Register(new ActionDefinition
		{
			Id = "view_art",
			Category = ActionCategory.Recreation,
			DisplayName = "EA_Action_view_art",
			Description = "EA_Action_view_art_Desc",
			DefaultPromptDesc = "View art piece for joy",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "target" },
			RiskLevel = RiskLevel.Low,
			Handler = new ViewArtHandler(),
			Keywords = new List<string> { "art", "sculpture", "painting", "艺术", "雕塑", "欣赏" }
		});
		Register(new ActionDefinition
		{
			Id = "use_comms",
			Category = ActionCategory.Facility,
			DisplayName = "EA_Action_use_comms",
			Description = "EA_Action_use_comms_Desc",
			DefaultPromptDesc = "Use communications console",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "target" },
			RiskLevel = RiskLevel.Low,
			Handler = new UseCommsHandler(),
			Keywords = new List<string> { "comms", "communicate", "radio", "通讯", "联络" }
		});
		Register(new ActionDefinition
		{
			Id = "trade",
			Category = ActionCategory.Facility,
			DisplayName = "EA_Action_trade",
			Description = "EA_Action_trade_Desc",
			DefaultPromptDesc = "Trade with visitor or caravan",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = new TradeHandler(),
			Keywords = new List<string> { "trade", "buy", "sell", "交易", "买卖", "贸易" }
		});
		Register(new ActionDefinition
		{
			Id = "enter_cryptosleep",
			Category = ActionCategory.Facility,
			DisplayName = "EA_Action_enter_cryptosleep",
			Description = "EA_Action_enter_cryptosleep_Desc",
			DefaultPromptDesc = "Enter cryptosleep casket",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = new EnterCryptosleepHandler(),
			Keywords = new List<string> { "cryptosleep", "casket", "hibernate", "低温休眠", "冬眠" }
		});
		Register(new ActionDefinition
		{
			Id = "reload",
			Category = ActionCategory.Facility,
			DisplayName = "EA_Action_reload",
			Description = "EA_Action_reload_Desc",
			DefaultPromptDesc = "Reload weapon or turret",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = new ReloadHandler(),
			Keywords = new List<string> { "reload", "ammo", "装填", "弹药" }
		});
		Register(new ActionDefinition
		{
			Id = "open_container",
			Category = ActionCategory.Facility,
			DisplayName = "EA_Action_open_container",
			Description = "EA_Action_open_container_Desc",
			DefaultPromptDesc = "Open container or casket",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = new OpenContainerHandler(),
			Keywords = new List<string> { "open", "container", "打开", "容器" }
		});
		Register(new ActionDefinition
		{
			Id = "bury",
			Category = ActionCategory.Funeral,
			DisplayName = "EA_Action_bury",
			Description = "EA_Action_bury_Desc",
			DefaultPromptDesc = "Bury corpse in grave",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = new BuryHandler(),
			Keywords = new List<string> { "bury", "grave", "funeral", "埋葬", "坟墓", "葬礼" }
		});
		Register(new ActionDefinition
		{
			Id = "cremate",
			Category = ActionCategory.Funeral,
			DisplayName = "EA_Action_cremate",
			Description = "EA_Action_cremate_Desc",
			DefaultPromptDesc = "Cremate corpse at crematorium",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = new CremateHandler(),
			Keywords = new List<string> { "cremate", "burn", "火化", "焚烧" }
		});
	}
}
