# SPEC-0001: Time Tracking (MVP)

Status: Draft
Owner: kYaRick
Last Updated: 2026-01-18

## Overview
Implement a minimal-yet-complete time tracking flow in a Blazor WebAssembly (standalone) app using MudBlazor for UI and modern vanilla CSS with CSS isolation.

## Functional Requirements
- Time Entry Form
  - Date: date picker, default value = today.
  - Project: dropdown with 3–5 hardcoded options (e.g., "Viso Internal", "Client A", "Client B", "Personal Development").
  - Hours: numeric input.
  - Work description: multiline textarea.
  - Save: appends a new entry to in-memory state initially.
- Entry History
  - List entries grouped by date.
  - Display columns: Date | Project | Hours | Description.
  - Totals: show total hours per day and a grand total across all entries.

## UI Components (MudBlazor Mapping)
- Date: `MudDatePicker` (default value = today)
- Project: `MudSelect<string>` with predefined items
- Hours: `MudNumericField<decimal>` (or `MudTextField<string>` with validation)
- Description: `MudTextField<string>` with `Lines` > 1
- Save: `MudButton` (variant: filled/contained)
- History: `MudTable` or grouped list with day headers and per-day totals

## Data Model
- TimeEntry
  - `Date` (DateOnly)
  - `Project` (string; one of the predefined options)
  - `Hours` (decimal)
  - `Description` (string)
  - `Id` ([UNKNOWN] whether a stable identifier is required at this stage)

## Validation
- Hours: numeric; min/max bounds [UNKNOWN].
- Required fields: [UNKNOWN] (assume Date, Project, Hours required; Description optional unless specified).
- Input formats and localization: [UNKNOWN].

## Accessibility (WCAG 2.2 AA)
- Label all inputs; provide appropriate `aria-` attributes.
- Logical tab order and visible focus indicators.
- Sufficient color contrast and clear error states.

## Non-Functional Requirements
- Performance: target AOT publishing; enable aggressive trimming; avoid reflection-heavy patterns.
- Styling: modern vanilla CSS, native nesting, custom properties; use CSS isolation.
- Testing: xUnit, bUnit (UI), NSubstitute, Shouldly.
- Automation: C#-based scripts runnable via `dotnet run`.

## Calculations
- Daily Total: sum of `Hours` for entries with the same `Date`.
- Grand Total: sum of `Hours` for all entries.

## Acceptance Criteria
- Default date is set to today when opening the form.
- User can select a project from 3–5 predefined options.
- Saving an entry immediately updates the in-memory list and the grouped history.
- History displays columns: Date | Project | Hours | Description.
- Daily totals and a grand total are visible and correctly computed.
- All UI elements are implemented with MudBlazor components.

## Open Questions
- Persistence: LocalStorage, IndexedDB, or backend API? [UNKNOWN]
- Validation bounds: allowed hours range, required fields, error messages [UNKNOWN]
- Stable IDs for entries and editing/deleting flows [UNKNOWN]
- Timezone/i18n handling for dates and numeric formatting [UNKNOWN]
- State management pattern preference (service, context, flux-like) [UNKNOWN]

## Out of Scope (This Spec)
- Authentication/authorization
- Synchronization with remote services
- Advanced reporting/exports
- Editing/deleting entries (unless added in a follow-up spec)

## Persistence & Hosting
### Persistence Options
- Default: IndexedDB in-browser storage for MVP on static hosting (GitHub Pages). Simple, offline-capable, minimal bundle impact.
- Optional: SQLite WASM in-browser for local SQL queries (stored in OPFS/IndexedDB). Heavier bundle and integration complexity; use only if SQL is required.
- Alternative: External backend API (e.g., ASP.NET Core + SQLite/Postgres) hosted separately; front-end remains static on GitHub Pages.

### Hosting (GitHub Pages)
- The app must support static hosting on GitHub Pages.
- Configure `base href` to the repository subpath (e.g., `/Viso.Tracker/`).
- Publish the built `wwwroot` to `docs/` and configure Pages to deploy from `main` → `docs/`.
