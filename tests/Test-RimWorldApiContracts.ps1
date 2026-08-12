param([Parameter(Mandatory=$true)][string]$RimWorldRoot)
$ErrorActionPreference='Stop'
$assemblyPath=Join-Path $RimWorldRoot 'RimWorldWin64_Data\Managed\Assembly-CSharp.dll'
if(-not(Test-Path -LiteralPath $assemblyPath)){throw "Assembly-CSharp missing: $assemblyPath"}
$assembly=[Reflection.Assembly]::LoadFrom($assemblyPath)
$requiredTypes=@('Verse.Pawn','Verse.Thing','Verse.Map','Verse.DefDatabase`1','Verse.AI.Pawn_JobTracker','Verse.AI.Job','Verse.JobMaker','RimWorld.JobDefOf','Verse.AI.ReservationManager','Verse.GenClosest')
foreach($name in $requiredTypes){if(-not $assembly.GetType($name,$false)){throw "RimWorld API type missing: $name"}}
$tracker=$assembly.GetType('Verse.AI.Pawn_JobTracker')
foreach($name in @('StartJob','EndCurrentJob','ClearQueuedJobs')){if(-not($tracker.GetMethods([Reflection.BindingFlags]'Public,NonPublic,Instance')|Where-Object Name -eq $name)){throw "Pawn_JobTracker method missing: $name"}}
'RIMWORLD_API_CONTRACT_OK types=10 methods=3 process_launches=0'
