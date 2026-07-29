# Claude Code — recovery after interruption / disconnection

## 1. Resume the session

Sessions are stored per-directory — you must resume from the SAME folder where the session
was started. Always start sessions from the project root so this is predictable:

```
cd /d C:\aspnet4\TruckCarrierHub
claude -c
```

- `claude -c` — continues the most recent session in this directory.
- `claude --resume` — shows a picker of past sessions in this directory.
- `claude --resume <session-id>` — resumes a specific one.

If Claude Code prints "This conversation is from a different directory. To resume, run: ..."
— its suggested command is PowerShell syntax. In a regular Command Prompt (cmd) rewrite it as:

```
cd /d C:\path\it\mentioned
claude --resume <session-id>
```

(cmd errors like "The filename, directory name, or volume label syntax is incorrect" mean you
pasted PowerShell quoting into cmd.)

## 2. First message after resuming — always this, before letting it continue

> What step are you on? List what you have completed and what remains. Do not do anything yet.

Never assume work was lost or finished — make it report first.

## 3. Verify its claims independently

In another terminal:

```
cd /d C:\aspnet4\TruckCarrierHub
git status --short
git diff --stat
```

- Check which files were actually modified and when (file timestamps).
- Ignore the ~747 files showing whitespace/line-ending-only changes — never let Claude Code
  stage or commit those; real work is always a handful of files.
- A leftover `.git\index.lock` with no running git process can be deleted safely.
- Leftover `*.rej` files mean a patch partially failed — tell Claude Code to delete them and
  re-check the affected file.

## 4. If the session can't be resumed ("No conversation found")

Start fresh in the project root and re-anchor it:

```
cd /d C:\aspnet4\TruckCarrierHub
claude
```

Then paste the original prompt file (the `*-prompt.md` in the solution root) with this preamble:

> This task was partially completed in a previous session that was lost. First run
> `git status --short` and `git diff --stat`, compare against the prompt below, report which
> changes are already in place, and continue from there. Do not redo completed work.

## 5. If it runs long (30+ min) with no visible progress

- Check file timestamps (`git status` + explorer) — if nothing changed recently it may be
  stuck measuring/verifying, not implementing.
- Press Esc once to interrupt (safe, keeps context) and ask what step it's on.
- Before killing anything DB-related, check SSMS for a long-running operation — killing a
  mid-flight `CREATE INDEX` causes a slow rollback:
  `SELECT session_id, command, percent_complete, wait_type FROM sys.dm_exec_requests WHERE session_id > 50`
- Known trap: never ask it to measure "before" timings of known-slow code paths — that runs
  the slow queries and can silently burn 30+ minutes. "After" timings only.

## 6. When it finishes

Make it end with: the exact list of files changed, build result, and the verification items
from the prompt. Then commit only those files.
