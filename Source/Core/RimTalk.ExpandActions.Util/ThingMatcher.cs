using System.Collections.Generic;
using System.Linq;
using RimTalk.ExpandActions.Integration;
using Verse;

namespace RimTalk.ExpandActions.Util;

public static class ThingMatcher
{
	private static bool _initialized;

	private static Dictionary<string, List<string>> _weaponKeywords;

	private static Dictionary<string, List<string>> _itemKeywords;

	public static void Initialize()
	{
		if (!_initialized)
		{
			_weaponKeywords = BuildWeaponKeywords();
			_itemKeywords = BuildItemKeywords();
			if (RJWIntegration.IsAvailable)
			{
				AddRJWKeywords();
				EALogger.Debug("[ThingMatcher] RJW keywords loaded");
			}
			_initialized = true;
			EALogger.Debug($"[ThingMatcher] Initialized with {_weaponKeywords.Count} weapon + {_itemKeywords.Count} item keywords");
		}
	}

	private static Dictionary<string, List<string>> BuildWeaponKeywords()
	{
		return new Dictionary<string, List<string>>
		{
			["刀"] = new List<string>
			{
				"刀", "knife", "blade", "匕首", "dagger", "短刀", "小刀", "shiv", "gladius", "砍刀",
				"machete", "柴刀"
			},
			["knife"] = new List<string> { "knife", "刀", "blade", "匕首", "dagger", "shiv" },
			["剑"] = new List<string> { "剑", "sword", "longsword", "长剑", "短剑", "gladius", "刀" },
			["sword"] = new List<string> { "sword", "剑", "longsword", "gladius", "blade" },
			["匕首"] = new List<string> { "匕首", "dagger", "小刀", "knife", "shiv" },
			["dagger"] = new List<string> { "dagger", "匕首", "knife", "shiv" },
			["斧"] = new List<string> { "斧", "axe", "斧头", "斧子", "hatchet", "战斧" },
			["axe"] = new List<string> { "axe", "斧", "hatchet", "斧头" },
			["棍"] = new List<string> { "棍", "棒", "club", "棍棒", "木棍", "铁棍", "警棍", "baton" },
			["club"] = new List<string> { "club", "棍", "棒", "baton" },
			["锤"] = new List<string> { "锤", "hammer", "锤子", "war hammer", "战锤", "mace", "钉头锤" },
			["hammer"] = new List<string> { "hammer", "锤", "mace", "war hammer" },
			["mace"] = new List<string> { "mace", "钉头锤", "锤", "club" },
			["矛"] = new List<string> { "矛", "spear", "长矛", "pike", "戟", "halberd", "长枪" },
			["spear"] = new List<string> { "spear", "矛", "pike", "长矛", "javelin" },
			["戟"] = new List<string> { "戟", "halberd", "矛", "pike" },
			["拳套"] = new List<string> { "拳套", "knuckle", "fist", "指虎", "brass knuckle" },
			["电击"] = new List<string> { "电击", "shock", "stun", "电棍", "电击棒", "zeushammer" },
			["人格"] = new List<string> { "人格", "persona", "传奇", "legendary" },
			["枪"] = new List<string>
			{
				"枪", "gun", "pistol", "rifle", "手枪", "步枪", "霰弹", "shotgun", "revolver", "左轮",
				"冲锋", "smg", "突击", "assault"
			},
			["gun"] = new List<string> { "gun", "枪", "pistol", "rifle", "firearm" },
			["手枪"] = new List<string> { "手枪", "pistol", "左轮", "revolver", "autopistol", "自动手枪" },
			["pistol"] = new List<string> { "pistol", "手枪", "revolver", "autopistol" },
			["左轮"] = new List<string> { "左轮", "revolver", "手枪", "pistol" },
			["revolver"] = new List<string> { "revolver", "左轮", "pistol" },
			["步枪"] = new List<string> { "步枪", "rifle", "突击步枪", "assault rifle", "狙击", "sniper", "卡宾", "carbine" },
			["rifle"] = new List<string> { "rifle", "步枪", "assault", "sniper", "carbine" },
			["狙击"] = new List<string> { "狙击", "sniper", "步枪", "rifle", "bolt" },
			["sniper"] = new List<string> { "sniper", "狙击", "bolt action", "rifle" },
			["突击"] = new List<string> { "突击", "assault", "突击步枪", "rifle", "自动" },
			["assault"] = new List<string> { "assault", "突击", "rifle", "automatic" },
			["冲锋"] = new List<string> { "冲锋", "smg", "冲锋枪", "submachine", "machine pistol", "微冲" },
			["smg"] = new List<string> { "smg", "冲锋", "submachine", "machine pistol" },
			["霰弹"] = new List<string> { "霰弹", "shotgun", "散弹", "pump" },
			["shotgun"] = new List<string> { "shotgun", "霰弹", "散弹", "pump" },
			["机枪"] = new List<string> { "机枪", "machine gun", "minigun", "加特林", "lmg", "hmg", "轻机枪", "重机枪" },
			["minigun"] = new List<string> { "minigun", "机枪", "加特林", "gatling" },
			["弓"] = new List<string> { "弓", "bow", "短弓", "长弓", "shortbow", "longbow", "greatbow", "大弓", "recurve" },
			["bow"] = new List<string> { "bow", "弓", "longbow", "shortbow", "recurve" },
			["弩"] = new List<string> { "弩", "crossbow", "十字弓" },
			["crossbow"] = new List<string> { "crossbow", "弩", "十字弓" },
			["手雷"] = new List<string> { "手雷", "grenade", "手榴弹", "molotov", "燃烧瓶", "emp", "frag" },
			["grenade"] = new List<string> { "grenade", "手雷", "手榴弹", "frag", "molotov" },
			["激光"] = new List<string> { "激光", "laser", "光束", "beam", "charge lance", "充能长矛" },
			["laser"] = new List<string> { "laser", "激光", "beam", "charge" },
			["充能"] = new List<string> { "充能", "charge", "plasma", "等离子", "能量" },
			["charge"] = new List<string> { "charge", "充能", "plasma", "energy" },
			["火箭"] = new List<string> { "火箭", "rocket", "launcher", "发射器", "导弹", "missile", "doomsday" },
			["rocket"] = new List<string> { "rocket", "火箭", "launcher", "missile" }
		};
	}

	private static Dictionary<string, List<string>> BuildItemKeywords()
	{
		return new Dictionary<string, List<string>>
		{
			["食物"] = new List<string>
			{
				"食物", "food", "meal", "肉", "meat", "蔬菜", "浆果", "berry", "饭", "餐",
				"营养膏"
			},
			["meal"] = new List<string>
			{
				"meal", "饭", "餐", "简单", "精致", "豪华", "simple meal", "fine meal", "lavish meal", "packaged",
				"survival"
			},
			["简单餐"] = new List<string> { "简单", "simple", "meal", "饭" },
			["精致餐"] = new List<string> { "精致", "fine", "meal", "饭" },
			["豪华餐"] = new List<string> { "豪华", "lavish", "meal", "饭" },
			["营养膏"] = new List<string> { "营养膏", "nutrient paste", "paste" },
			["生存餐"] = new List<string> { "生存", "survival", "packaged", "打包" },
			["肉"] = new List<string> { "肉", "meat", "生肉", "raw", "人肉", "human meat", "虫肉", "insect" },
			["浆果"] = new List<string> { "浆果", "berry", "berries", "果实", "草莓" },
			["蔬菜"] = new List<string>
			{
				"蔬菜", "vegetable", "蔬", "菜", "玉米", "corn", "土豆", "potato", "水稻", "rice",
				"草莓", "haygrass", "干草"
			},
			["玉米"] = new List<string> { "玉米", "corn", "蔬菜" },
			["土豆"] = new List<string> { "土豆", "potato", "蔬菜" },
			["水稻"] = new List<string> { "水稻", "rice", "稻米", "蔬菜" },
			["蘑菇"] = new List<string> { "蘑菇", "mushroom", "fungus", "菌" },
			["药"] = new List<string> { "药", "medicine", "drug", "医药", "草药", "herbal", "工业医药", "闪耀世界医药", "glitterworld" },
			["medicine"] = new List<string> { "medicine", "药", "医药", "medical", "herbal" },
			["草药"] = new List<string> { "草药", "herbal", "herbal medicine", "草本" },
			["工业医药"] = new List<string> { "工业", "industrial", "medicine", "医药" },
			["闪耀世界医药"] = new List<string> { "闪耀", "glitterworld", "medicine", "医药", "高级" },
			["绷带"] = new List<string> { "绷带", "bandage", "医疗" },
			["毒品"] = new List<string>
			{
				"毒品", "drug", "drugs", "瘾品", "啤酒", "beer", "烟", "smoke", "yayo", "flake",
				"go-juice", "wake-up", "luciferium"
			},
			["啤酒"] = new List<string> { "啤酒", "beer", "酒", "alcohol" },
			["烟叶"] = new List<string> { "烟", "smoke", "smokeleaf", "joint", "叶", "tobacco" },
			["迷幻药"] = new List<string> { "yayo", "迷幻", "可卡因", "cocaine", "flake" },
			["兴奋剂"] = new List<string> { "go-juice", "兴奋", "juice", "wake-up", "提神" },
			["恶魔素"] = new List<string> { "luciferium", "恶魔", "魔素", "lucy" },
			["钢"] = new List<string> { "钢", "steel", "钢铁", "铁", "metal" },
			["steel"] = new List<string> { "steel", "钢", "钢铁", "metal" },
			["铁"] = new List<string> { "铁", "iron", "steel", "钢" },
			["银"] = new List<string> { "银", "silver", "白银" },
			["silver"] = new List<string> { "silver", "银", "白银" },
			["金"] = new List<string> { "金", "gold", "黄金" },
			["gold"] = new List<string> { "gold", "金", "黄金" },
			["铀"] = new List<string> { "铀", "uranium", "放射" },
			["uranium"] = new List<string> { "uranium", "铀" },
			["塑钢"] = new List<string> { "塑钢", "plasteel", "合金" },
			["plasteel"] = new List<string> { "plasteel", "塑钢" },
			["玉"] = new List<string> { "玉", "jade", "翡翠" },
			["jade"] = new List<string> { "jade", "玉", "翡翠" },
			["石"] = new List<string>
			{
				"石", "stone", "岩石", "石块", "granite", "花岗岩", "marble", "大理石", "slate", "板岩",
				"sandstone", "砂岩", "limestone", "石灰岩"
			},
			["stone"] = new List<string> { "stone", "石", "block", "chunk", "rock" },
			["花岗岩"] = new List<string> { "花岗岩", "granite", "石" },
			["大理石"] = new List<string> { "大理石", "marble", "石" },
			["板岩"] = new List<string> { "板岩", "slate", "石" },
			["木材"] = new List<string> { "木材", "wood", "木头", "原木", "log", "木" },
			["wood"] = new List<string> { "wood", "木材", "木头", "log" },
			["布"] = new List<string> { "布", "cloth", "fabric", "棉", "cotton", "布料" },
			["cloth"] = new List<string> { "cloth", "布", "fabric", "cotton" },
			["丝"] = new List<string> { "丝", "silk", "丝绸", "蚕丝" },
			["silk"] = new List<string> { "silk", "丝", "丝绸" },
			["羊毛"] = new List<string> { "羊毛", "wool", "毛", "alpaca", "羊驼毛", "muffalo", "野牛毛" },
			["wool"] = new List<string> { "wool", "羊毛", "毛" },
			["合成纤维"] = new List<string> { "合成纤维", "synthread", "hyperweave", "超织", "devilstrand", "恶魔布" },
			["synthread"] = new List<string> { "synthread", "合成纤维", "synthetic" },
			["hyperweave"] = new List<string> { "hyperweave", "超织", "超级纤维" },
			["恶魔布"] = new List<string> { "恶魔布", "devilstrand", "恶魔", "devil" },
			["皮"] = new List<string> { "皮", "leather", "皮革", "毛皮", "fur", "skin", "hide" },
			["leather"] = new List<string> { "leather", "皮", "皮革", "hide" },
			["毛皮"] = new List<string> { "毛皮", "fur", "皮", "pelt" },
			["零件"] = new List<string> { "零件", "component", "组件", "零部件", "parts" },
			["component"] = new List<string> { "component", "零件", "组件" },
			["高级组件"] = new List<string> { "高级组件", "advanced component", "高级", "advanced" },
			["AI核心"] = new List<string> { "AI", "核心", "ai core", "persona core", "人格核心" },
			["衣服"] = new List<string> { "衣服", "apparel", "clothes", "clothing", "服装", "穿戴" },
			["帽子"] = new List<string> { "帽子", "hat", "cap", "帽", "头盔", "helmet", "hood", "兜帽" },
			["头盔"] = new List<string> { "头盔", "helmet", "盔", "power armor helmet", "动力头盔" },
			["夹克"] = new List<string> { "夹克", "jacket", "外套", "coat", "大衣", "parka", "派克" },
			["裤子"] = new List<string> { "裤子", "pants", "裤", "trousers" },
			["衬衫"] = new List<string> { "衬衫", "shirt", "上衣", "t-shirt" },
			["背心"] = new List<string> { "背心", "vest", "防弹", "flak", "护甲" },
			["护甲"] = new List<string> { "护甲", "armor", "装甲", "plate", "flak", "防弹", "marine", "海军陆战队", "power armor", "动力装甲" },
			["动力装甲"] = new List<string> { "动力装甲", "power armor", "power armour", "动力", "cataphract", "重装" },
			["海军陆战队"] = new List<string> { "海军陆战队", "marine", "marine armor", "陆战队" },
			["防护服"] = new List<string> { "防护服", "protective", "shield belt", "护盾", "shield" },
			["披风"] = new List<string> { "披风", "cape", "cloak", "斗篷" },
			["手套"] = new List<string> { "手套", "glove", "gloves", "gauntlet" },
			["靴子"] = new List<string> { "靴子", "boot", "boots", "鞋", "shoes" },
			["尸体"] = new List<string> { "尸体", "corpse", "body", "遗体", "dead" },
			["骨头"] = new List<string> { "骨头", "bone", "skull", "骷髅", "骨骼" },
			["床"] = new List<string> { "床", "bed", "sleeping", "睡", "bedroll", "睡袋", "royal bed", "皇家床", "double bed", "双人床" },
			["椅子"] = new List<string> { "椅子", "chair", "stool", "凳", "armchair", "扶手椅", "throne", "王座" },
			["桌子"] = new List<string> { "桌子", "table", "desk", "台", "工作台", "workbench" },
			["灯"] = new List<string> { "灯", "lamp", "light", "照明", "standing lamp", "落地灯", "sun lamp", "太阳灯" },
			["门"] = new List<string> { "门", "door", "gate", "autodoor", "自动门" },
			["墙"] = new List<string> { "墙", "wall", "墙壁" },
			["炮塔"] = new List<string> { "炮塔", "turret", "gun turret", "防御塔", "minigun turret", "机枪炮塔" },
			["电池"] = new List<string> { "电池", "battery", "蓄电池", "电力" },
			["发电机"] = new List<string> { "发电机", "generator", "power", "solar", "太阳能", "wind", "风力", "geothermal", "地热" },
			["空调"] = new List<string> { "空调", "cooler", "heater", "加热器", "冷却器", "温度" },
			["奴隶项圈"] = new List<string> { "奴隶", "slave", "collar", "项圈", "slavery" },
			["神经训练器"] = new List<string> { "神经", "neurotrainer", "trainer", "训练器", "技能训练" },
			["仿生"] = new List<string> { "仿生", "bionic", "prosthetic", "假肢", "义肢", "implant", "植入物" },
			["古物"] = new List<string> { "古物", "artifact", "古代", "ancient", "relic", "遗物" }
		};
	}

	private static void AddRJWKeywords()
	{
		foreach (KeyValuePair<string, List<string>> item in new Dictionary<string, List<string>>
		{
			["束缚"] = new List<string> { "束缚", "bondage", "restraint", "枷锁", "镣铐", "cuff", "chain" },
			["项圈"] = new List<string> { "项圈", "collar", "奴隶项圈", "slave collar" },
			["避孕"] = new List<string> { "避孕", "contraceptive", "condom", "套" }
		})
		{
			_itemKeywords[item.Key] = item.Value;
		}
	}

	public static IEnumerable<Thing> FindMatching(IEnumerable<Thing> things, string searchTerm, bool weaponOnly = false)
	{
		EnsureInitialized();
		if (string.IsNullOrEmpty(searchTerm))
		{
			return Enumerable.Empty<Thing>();
		}
		string text = searchTerm.ToLower().Trim();
		List<(Thing, int)> list = new List<(Thing, int)>();
		List<string> expandedKeywords = GetExpandedKeywords(text, weaponOnly);
		foreach (Thing thing in things)
		{
			if (!weaponOnly || thing.def.IsWeapon)
			{
				int num = CalculateMatchScore(thing, text, expandedKeywords);
				if (num > 0)
				{
					list.Add((thing, num));
				}
			}
		}
		return from r in list
			orderby r.Item2 descending
			select r.Item1;
	}

	private static List<string> GetExpandedKeywords(string term, bool weaponOnly)
	{
		EnsureInitialized();
		List<string> list = new List<string> { term };
		if (_weaponKeywords.TryGetValue(term, out var value))
		{
			list.AddRange(value);
		}
		if (!weaponOnly && _itemKeywords.TryGetValue(term, out var value2))
		{
			list.AddRange(value2);
		}
		return list.Distinct().ToList();
	}

	private static int CalculateMatchScore(Thing thing, string searchTerm, List<string> keywords)
	{
		string text = thing.Label?.ToLower() ?? "";
		string text2 = thing.def?.defName?.ToLower() ?? "";
		string text3 = thing.def?.label?.ToLower() ?? "";
		int num = 0;
		if (text == searchTerm || text3 == searchTerm)
		{
			return 100;
		}
		if (text.StartsWith(searchTerm) || text3.StartsWith(searchTerm))
		{
			num += 50;
		}
		if (text.Contains(searchTerm) || text3.Contains(searchTerm))
		{
			num += 30;
		}
		if (text2.Contains(searchTerm))
		{
			num += 20;
		}
		foreach (string keyword in keywords)
		{
			if (!(keyword == searchTerm))
			{
				if (text.Contains(keyword) || text3.Contains(keyword))
				{
					num += 15;
				}
				if (text2.Contains(keyword))
				{
					num += 10;
				}
			}
		}
		return num;
	}

	public static bool IsMatch(Thing thing, string searchTerm, bool weaponOnly = false)
	{
		EnsureInitialized();
		if (string.IsNullOrEmpty(searchTerm) || thing == null)
		{
			return false;
		}
		if (weaponOnly && !thing.def.IsWeapon)
		{
			return false;
		}
		string text = searchTerm.ToLower().Trim();
		List<string> expandedKeywords = GetExpandedKeywords(text, weaponOnly);
		return CalculateMatchScore(thing, text, expandedKeywords) > 0;
	}

	public static string GetSearchDebugInfo(string searchTerm, bool weaponOnly = false)
	{
		EnsureInitialized();
		List<string> expandedKeywords = GetExpandedKeywords(searchTerm?.ToLower().Trim() ?? "", weaponOnly);
		return "Search '" + searchTerm + "' expands to: [" + string.Join(", ", expandedKeywords.Take(8)) + ((expandedKeywords.Count > 8) ? "..." : "") + "]";
	}

	private static void EnsureInitialized()
	{
		if (!_initialized)
		{
			Initialize();
		}
	}
}
