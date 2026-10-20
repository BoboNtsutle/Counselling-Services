# How we work on GitHub

**The rule:** nobody pushes straight to `main`. Everyone works on their own branch and sends a
**Pull Request (PR)**, and someone else checks it before it's merged.

---

## 1. Lead (once) · create the repo

1. On github.com → **New repository** → name `ancora-web` → **Private** → *don't* add a README (we have one).
2. On your laptop, inside the unzipped `ancora-web` folder:
   ```bash
   git init
   git add .
   git commit -m "Starter: ASP.NET Core MVC app connected to Supabase"
   git branch -M main
   git remote add origin https://github.com/<your-username>/ancora-web.git
   git push -u origin main
   ```
3. **Settings → Collaborators → Add people** and add the other 5 GitHub usernames.
4. **Settings → Branches → Add branch ruleset** (or *Add rule*) for `main`:
   - ✅ Require a pull request before merging, with **1 approval**
   - ✅ Block force pushes
5. Optional but useful: **Projects → New project → Board**. Make one card per checkbox in `TEAM-PLAN.md`.

## 2. Everyone (once) · get the code

1. Accept the email invite from GitHub.
2. Clone:
   - **Visual Studio:** *Clone a repository* → paste the repo URL → open `AncoraWeb.sln`
   - **Terminal:** `git clone https://github.com/<lead-username>/ancora-web.git`
3. Press **F5** and log in to check it runs.

## 3. Every time you work

```bash
git checkout main
git pull                                  # get everyone's latest work
git checkout -b m2-approve-reject         # new branch: <module>-<what you're doing>
# ... code, run, test ...
git add .
git commit -m "M2: approve and reject appointments"
git push -u origin m2-approve-reject
```

Then on GitHub click **Compare & pull request**:
- Title: `M2: approve and reject appointments`
- Description: what you did, which TEAM-PLAN boxes it ticks, and a **screenshot**
- Reviewers: the lead + one other person

After it's merged, go back to `main`, `git pull`, and start a new branch for the next piece.

In **Visual Studio** you can do all of this from the **Git Changes** window: *New Branch* → *Commit All* → *Push* → *Create Pull Request*.

## 4. Rules that prevent conflicts

| ✅ Do | ❌ Don't |
|---|---|
| Only edit the files listed for your module in `TEAM-PLAN.md` | Edit someone else's controller/view "quickly" |
| Small PRs (one feature each), pushed often | One huge PR at the end |
| `git pull` on main before starting a new branch | Work on `main` directly |
| Ask in the group chat if you need a shared file changed | Change `Program.cs`, `_Layout.cshtml`, `site.css`, `Models/*` or `Services/Supabase/*` yourself. Ask the lead. |
| Reuse the CSS classes in `site.css` (`card-a`, `table-a`, `btn-ancora`, `badge-a` …) | Paste new colours. Use the `var(--navy)` style variables. |

Need a new column in a model (e.g. `Appointment.cs`)? Ask the lead, so everybody gets the same change.

## 5. If you get a merge conflict

1. `git checkout main && git pull`
2. `git checkout your-branch && git merge main`
3. Visual Studio shows the conflicting files. Pick *Current*, *Incoming* or both, then save.
4. `git add . && git commit -m "Merge main" && git push`

Stuck? Don't force-push. Ask the lead.

## 6. Reviewing someone's PR

- Pull their branch, run it, click through their pages
- Check: does it work for **both** the admin and psychologist accounts where relevant?
- No passwords or secret keys in the code
- Approve, or leave a comment saying exactly what to change

## 7. Never commit

- Real passwords or test logins (share them in the group chat)
- The Supabase **service_role / secret** key or database password
- `bin/` and `obj/` folders (the `.gitignore` already blocks these)
