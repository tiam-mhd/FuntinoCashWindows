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
