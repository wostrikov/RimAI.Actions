using System;
using System.Collections.Generic;
using System.Linq;
using Ustas.RimAI.Actions.Util;
using Verse;

namespace Ustas.RimAI.Actions.Execution;

public static class PawnResolver
{
	private static readonly Dictionary<string, string[]> RoomNameAliases = new Dictionary<string, string[]>
	{
		{
			"DiningRoom",
			new string[4] { "їдальня", "ресторан", "обідня", "dining" }
		},
		{
			"Bedroom",
			new string[4] { "спальня", "кімната", "опочивальня", "bedroom" }
		},
		{
			"Barracks",
			new string[4] { "казарма", "барак", "гуртожиток", "barracks" }
		},
		{
			"Hospital",
			new string[4] { "лікарня", "медпункт", "палата", "hospital" }
		},
		{
			"PrisonCell",
			new string[4] { "камера", "вʼязниця", "буцегарня", "prison" }
		},
		{
			"PrisonBarracks",
			new string[2] { "Барак для бранців", "prison barracks" }
		},
		{
			"Laboratory",
			new string[3] { "лабораторія", "дослідна", "lab" }
		},
		{
			"RecRoom",
			new string[4] { "кімната відпочинку", "вітальня", "rec room", "recreation" }
		},
		{
			"Tomb",
			new string[3] { "склеп", "усипальня", "tomb" }
		},
		{
			"Kitchen",
			new string[3] { "кухня", "куховарня", "kitchen" }
		},
		{
			"Workshop",
			new string[4] { "цех", "майстерня", "робітня", "workshop" }
		},
		{
			"Barn",
			new string[3] { "загін", "хлів", "barn" }
		},
		{
			"ThroneRoom",
			new string[3] { "тронна зала", "зала прийомів", "throne" }
		},
		{
			"WorshipRoom",
			new string[5] { "каплиця", "святилище", "храм", "temple", "worship" }
		}
	};

	public static Pawn ResolvePawn(string name, Map map)
	{
		if (string.IsNullOrWhiteSpace(name) || map == null)
		{
			return null;
		}
		List<Pawn> source = map.mapPawns.AllPawns.Where((Pawn p) => p.Name != null).ToList();
		List<Pawn> list = source.Where((Pawn p) => p.Name.ToStringFull == name).ToList();
		if (list.Count == 1)
		{
			return list[0];
		}
		if (list.Count > 1)
		{
			EALogger.Warn("Ambiguous pawn name (full): " + name);
			return null;
		}
		List<Pawn> list2 = source.Where((Pawn p) => MatchesNickname(p.Name, name)).ToList();
		if (list2.Count == 1)
		{
			return list2[0];
		}
		if (list2.Count > 1)
		{
			EALogger.Warn("Ambiguous pawn name (nickname): " + name);
			return null;
		}
		List<Pawn> list3 = source.Where((Pawn p) => p.Name.ToStringFull.Contains(name) || name.Contains(p.Name.ToStringShort)).ToList();
		if (list3.Count == 1)
		{
			return list3[0];
		}
		EALogger.Debug("Pawn not found: " + name);
		return null;
	}

	public static Thing ResolveThing(string keyword, Map map, IntVec3? nearCell = null)
	{
		if (string.IsNullOrWhiteSpace(keyword) || map == null)
		{
			return null;
		}
		List<Thing> list = new List<Thing>();
		foreach (Thing allThing in map.listerThings.AllThings)
		{
			if (allThing.def.defName.Contains(keyword) || allThing.Label.Contains(keyword) || allThing.LabelCap.ToString().Contains(keyword))
			{
				list.Add(allThing);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		if (nearCell.HasValue)
		{
			return list.OrderBy((Thing t) => t.Position.DistanceTo(nearCell.Value)).FirstOrDefault();
		}
		return list.FirstOrDefault();
	}

	public static IntVec3? ResolveRoom(string keyword, Map map, IntVec3? nearCell = null)
	{
		if (string.IsNullOrWhiteSpace(keyword) || map == null)
		{
			return null;
		}
		string text = keyword.ToLowerInvariant();
		HashSet<Room> hashSet = new HashSet<Room>();
		foreach (Region allRegion in map.regionGrid.AllRegions)
		{
			Room room = allRegion.Room;
			if (room != null && !room.TouchesMapEdge && room.Role != null)
			{
				hashSet.Add(room);
			}
		}
		Room room2 = null;
		float num = float.MaxValue;
		foreach (Room item in hashSet)
		{
			bool flag = false;
			string defName = item.Role.defName;
			string text2 = item.Role.label?.ToLowerInvariant() ?? "";
			if (text2.Contains(text) || text.Contains(text2))
			{
				flag = true;
			}
			if (!flag && RoomNameAliases.TryGetValue(defName, out var value))
			{
				string[] array = value;
				foreach (string text3 in array)
				{
					if (keyword.Contains(text3) || text3.Contains(keyword))
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				IntVec3 a = item.Cells.FirstOrDefault();
				float num2 = (nearCell.HasValue ? a.DistanceTo(nearCell.Value) : 0f);
				if (num2 < num)
				{
					num = num2;
					room2 = item;
				}
			}
		}
		if (room2 != null)
		{
			foreach (IntVec3 cell in room2.Cells)
			{
				if (cell.Standable(map))
				{
					EALogger.Debug($"Resolved room '{keyword}' -> {room2.Role.defName} at {cell}");
					return cell;
				}
			}
		}
		return null;
	}

	public static IntVec3? ParseCell(string cellStr)
	{
		if (string.IsNullOrWhiteSpace(cellStr))
		{
			return null;
		}
		string[] array = cellStr.Split(new[] { ',' }, StringSplitOptions.None);
		if (array.Length != 2)
		{
			return null;
		}
		if (int.TryParse(array[0].Trim(), out var result) && int.TryParse(array[1].Trim(), out var result2))
		{
			return new IntVec3(result, 0, result2);
		}
		return null;
	}

	private static bool MatchesNickname(Name name, string query)
	{
		if (name == null)
		{
			return false;
		}
		if (name.ToStringShort == query)
		{
			return true;
		}
		if (name is NameTriple nameTriple && nameTriple.Nick == query)
		{
			return true;
		}
		return false;
	}
}
