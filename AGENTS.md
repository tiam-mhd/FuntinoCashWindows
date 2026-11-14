# Mandatory implementation rules

These rules apply to every current and future change in this repository.

## Access control

- Every new screen, data source, command, export, or write operation must have an explicit permission code in FuntinoControlCenter/Security/PermissionCatalog.cs. Permission names, group names, and explanations shown to users must be clear Persian.
- Permission checks must be fail-closed and enforced both when opening/showing a screen and inside the repository or service method that reads or changes protected data. Hiding a navigation button alone is never authorization.
- If the current user lacks access to a screen or operation, do not show its controls, fields, data, or navigation entry. Do not substitute a disabled control for a hidden unauthorized section.
- For table data, define and enforce separate view, create, update, and delete permissions for its business schema. Exports require the export permission as well as permission to view the underlying data.
- Role assignments and role-permission changes must invalidate or refresh affected active sessions before their next protected operation. Never authorize from a stale administrator flag or a UI-only check.
- Changes to accounts, roles, assignments, and permissions must be auditable. Protect the last active system administrator and do not allow regular role management to remove or edit the built-in system-administrator role.
- A newly created or password-reset account receives a random one-time password, is required to change it at the next login, and must be shown that secret only once in a copyable dialog. Never log or persist the clear-text password.
- A user may change only their own password. A forced password change must complete before the application opens any other protected screen.
- The pre-login connection setup is a limited bootstrap exception: it may change SQL connection settings needed to establish the initial connection, but must not expose business data, user administration, hardware settings, or other authenticated screens.

## Database changes

- Apply application-owned schema changes as additive, idempotent migrations in the app schema. Preserve existing accounts and business data.
- Keep the reference MDF/LDF inputs immutable. Perform schema and data migrations only on the installed operational copies and record completion transactionally.
- Update migration history, permission catalog captions, persistence, UI visibility, and operation guards together whenever a protected capability is added or changed.

## Localization

- User-facing forms and permission descriptions must be Persian, understandable to nontechnical users, and laid out for right-to-left reading.

## WinForms design files

- All WinForms visual construction and fixed design belong in the form's `.Designer.cs`: control declarations and creation, the control tree, layout containers, dimensions, positions, docking, anchors, fonts, colors, static captions, initial visual properties, and designer-managed event hookups.
- Form `.cs` files contain behavior only: event handlers, permission decisions, data binding, and runtime values or visibility that depend on the current user or data. They must not create controls, add controls to containers, or set fixed layout or styling properties.
- Every form must remain a `partial` class with its matching `.cs` and `.Designer.cs` included in the project and associated with `DependentUpon`, so Visual Studio can open and edit it in the Windows Forms Designer.
- When adding or changing a form control, update the `.Designer.cs`; do not replace designer-owned controls with runtime-created equivalents.

## Professional UI/UX redesign

- When the user asks for a "بازطراحی فوق‌حرفه‌ای" (professional redesign) or equivalent, apply this complete redesign standard to every requested screen. The standard may be extended by later user instructions.
- Arrange each screen as a coherent, polished experience with a consistent visual system, clear hierarchy, balanced spacing, and colors suited to the product and task.
- Translate every user-facing English or technical phrase into simple, natural Persian that an everyday user can understand. Preserve technical identifiers only where users need to recognize them.
- Use Vazirmatn for interface text. Bundle the font when the target system may not have it installed.
- Give each requested page an attractive, page-appropriate background generated with the image-generation skill. Generate and integrate custom WebP icons for every operation on the requested screens.
- Route each create, read, update, or delete operation to a dedicated page or form. If a requested screen contains a table, implement real search, filters, pagination, and useful table states along with a polished, usable table design; do not add decorative or nonfunctional controls.
- Buttons with white or light backgrounds must have a visible border so their labels and icons read as part of the button; use rounded corners for buttons where the control supports them.
- Text boxes must use a background coordinated with their containing card or page and have rounded, bordered edges so they remain visually distinct and readable.
- Keep all WinForms visual construction, layout, fixed styling, captions, and designer-managed event hookups in `.Designer.cs`, in accordance with the WinForms design-file rules above. Keep runtime behavior in `.cs` files.

## Card operations landing and device ports

- After sign-in, the first page is the card operations hub. Give card top-up and card balance the strongest visual priority, with shortcuts to the card-related operations available to the signed-in user.
- Port settings must show only ports that pass the relevant device test. With one successful port, select it automatically; with several, let the user choose; with none, show a clear Persian message and keep the list empty.
- A serial port that opens and closes successfully is only known to be available to Windows. Do not describe that result as proof that the card reader responded or that card reading works. Use a device-specific handshake when the hardware supports one, and state the test boundary in Persian.
- POS port detection must initialize the installed SDK and accept only its documented success response. Missing SDKs or unsuccessful responses must never appear in the successful-port list.

<!-- gitnexus:start -->
# GitNexus — Code Intelligence

This project is indexed by GitNexus as **FuntinoCashWindows** (645 symbols, 1835 relationships, 54 execution flows).

> Index stale? Run `node .gitnexus/run.cjs analyze --index-only` from the project root — it auto-selects an available runner. No `.gitnexus/run.cjs` yet? Bootstrap with `npx`, `bunx`, or `pnpm dlx` — e.g. `bunx gitnexus@latest analyze` (npm 11 npx crash; #1939).

## Always Do

- **MUST run impact before editing.** Use `impact({target: "symbolName", direction: "upstream"})` or `node .gitnexus/run.cjs impact "symbolName" --direction upstream --repo .`; report callers, processes, and risk. Never substitute grep for graph analysis.
- **MUST analyze graph changes before committing.** Use `detect_changes({scope: "all"})` (MCP) or `node .gitnexus/run.cjs detect-changes --scope all --repo .` (CLI fallback). `partial: true` or `truncated: true` is not a clean check — a zero means unseen, not unaffected; re-run it. For regression review: `detect_changes({scope: "compare", base_ref: "main"})` or `node .gitnexus/run.cjs detect-changes --scope compare --base-ref "main" --repo .`.
- MUST warn on HIGH/CRITICAL `risk` pre-edit; never use `riskSharedAxes` to waive a HIGH/CRITICAL `risk` warning. Compare File/symbol: MCP File omits axes; Graph-RAG expands File.
- **MUST treat `risk: UNKNOWN` as unresolved, not as low.** An empty caller set is not evidence the symbol is unused — it can also mean the callers are not resolvable by the index (plain-object property access, dynamic dispatch, cross-language calls). `impact` pairs `UNKNOWN` with a `riskNote` saying so. Confirm with a text search before treating the symbol as safe to change or delete; do not proceed on the strength of a zero.
- **MUST use `query({search_query: "concept"})` for concepts/flows, `context({name: "symbolName"})` for a named symbol, or `impact` for blast radius, on read-only callers, dependencies, imports, or execution flow.** Graph first; text search only for empty/`UNKNOWN`/literals.
- For security review, `explain({target: "fileOrSymbol"})` lists taint findings (source→sink flows; needs `analyze --pdg`).

## Never Do

- NEVER edit a function, class, or method before MCP/CLI impact analysis.
- NEVER ignore HIGH or CRITICAL risk warnings from impact analysis, and never read `UNKNOWN` as an all-clear — it means the walk could not answer, which is the one verdict that requires confirming by other means.
- NEVER rename symbols with find-and-replace — use `rename` which understands the call graph.
- NEVER commit before MCP/CLI graph change analysis.

## Resources

| Resource | Use for |
| --- | --- |
| `gitnexus://repo/FuntinoCashWindows/context` | Codebase overview, check index freshness |
| `gitnexus://repo/FuntinoCashWindows/clusters` | All functional areas |
| `gitnexus://repo/FuntinoCashWindows/processes` | All execution flows |
| `gitnexus://repo/FuntinoCashWindows/process/{name}` | Step-by-step execution trace |

## CLI

| Task | Read this skill file |
| --- | --- |
| Understand architecture / "How does X work?" | `.claude/skills/gitnexus-exploring/SKILL.md` |
| Blast radius / "What breaks if I change X?" | `.claude/skills/gitnexus-impact-analysis/SKILL.md` |
| Trace bugs / "Why is X failing?" | `.claude/skills/gitnexus-debugging/SKILL.md` |
| Rename / extract / split / refactor | `.claude/skills/gitnexus-refactoring/SKILL.md` |
| Tools, resources, schema reference | `.claude/skills/gitnexus-guide/SKILL.md` |
| Index, status, clean, wiki CLI commands | `.claude/skills/gitnexus-cli/SKILL.md` |

<!-- gitnexus:end -->
