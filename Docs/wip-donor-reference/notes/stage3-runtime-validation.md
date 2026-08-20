# Stage 3 runtime validation: pawn action lifecycle

Do not activate the original Workshop implementation and a local build together.

Test dialogue with no action, movement, work, successful action, and failed/cancelled
action. For every case verify that the pawn resumes normal AI, has no permanent busy
flag or job lock, the EA queue drains correctly, draft/undraft recovery is unnecessary,
and `Player.log` contains no new errors. Include a `stop` action followed by a queued
movement/work job to specifically confirm that ending the current job does not stop the
replacement job's pather.
