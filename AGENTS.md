# AGENT MANIFEST & OPERATIONAL GUIDELINES (SDD-DRIVEN)

## 1. PROJECT VISION & TECHNICAL ECOSYSTEM
* **Runtime:** .NET 10 (Blazor WebAssembly Standalone).
* **Language:** C# 14 (Strict Nullable Reference Types, Zero-Warning Policy).
* **UI Framework:** **MudBlazor (Latest)** — Mandatory. Use MudBlazor components for all UI. Prioritize native components and the built-in theming engine.
* **Styling:** **Modern Vanilla CSS**. Use Native Nesting, Custom Properties (Variables), and Container Queries. **SCSS/Sass is prohibited**. Mandate **CSS Isolation** (`.razor.css`) for all components.
* **Testing Suite:** xUnit, bUnit (UI), NSubstitute (Mocking), and Shouldly (Assertions).
* **Automation:** **C# for DevOps**. All CI/CD, GitHub Actions, and utility scripts must be implemented in C# and executed via `dotnet run`.
* **Performance:** Optimized for **AOT** and **Aggressive Trimming**. Use Source Generators for JSON and Logging; avoid Reflection.

---

## 2. REPOSITORY HIERARCHY & AUTHORITY
| Path | Governance | Agent Permissions |
| :--- | :--- | :--- |
| `README.md` | General Identity | **Read-Only**. |
| `specs/` | **Technical Source of Truth** | **Read-Only**. Absolute authority for logic and architecture. |
| `docs/` | Human Strategic Logic | **Read-Only**. Strategic business rules. |
| `docs/.agents/` | AI Sandbox | **Full Access**. Storage for logs, tech-docs, and proposals. |

---

## 3. SDD OPERATIONAL PROTOCOL (METHODOLOGY)
1.  **Synchronization:** Re-analyze `specs/` and the current codebase before initiating any task.
2.  **Constraint Enforcement:** Code changes must strictly align with the specifications in `specs/`. If a task conflicts with a spec, stop and report.
3.  **Gap Analysis:** If logic is missing from `specs/`, do not assume. Flag as `[UNKNOWN]` and request human clarification.
4.  **Self-Evolution:** Proactively propose updates to this manifest and project dependencies to maintain 2026 industry benchmarks.
5.  **Verification Gate:** Tasks are "Done" only if they pass all tests, contain no compiler warnings, and satisfy WCAG 2.2 accessibility standards.

---

## 4. CODE CRAFTSMANSHIP & DOCUMENTATION
* **Documentation Standards:**
    * **English language only.**
    * **Public Members:** XML `<summary>` tags mandatory.
    * **Internal Logic:** Minimize comments. Use only to explain "Why" (intent), not "What" (action).
    * **Clean Code:** Prioritize self-documenting identifiers and `GlobalUsings.cs` for noise reduction.
* **C# 14 Usage:** Actively utilize Primary Constructors, Interceptors, and Collection Expressions.
* **Architecture:** Maintain **Static Web Asset (SWA)** compatibility.

---

## 5. AUTOMATION & DEVOPS
* **Scripting:** All development and deployment automation must be C#-based.
* **Execution:** Scripts must be executable via `dotnet run` (e.g., `dotnet run scripts/BuildUtility.cs`).
* **Integrity:** Automation logic must be as modular and well-tested as the primary application.

---

## 6. AGENT OUTPUTS (`docs/.agents/`)
* **`/logs/`**: SDD validation reports (checklist of implemented spec requirements) and test results.
* **`/tech/`**: Auto-generated technical deep-dives and architectural diagrams (Mermaid.js).
* **`/refactoring/`**: Proactive proposals for modernization and dependency updates.

---

## 7. FEATURE SCOPE (TIME TRACKING)
This iteration targets a minimal-yet-complete time tracking flow.

### 7.1 Time Entry Form
* Date — date picker (default: today).
* Project — dropdown with 3–5 hardcoded options (e.g., "Viso Internal", "Client A", "Client B", "Personal Development").
* Hours — numeric input for number of hours.
* Work description — multiline textarea.
* Save button — appends a new entry to in-memory state initially.

### 7.2 Entry History
* List of entries grouped by date.
* Display columns: Date | Project | Hours | Description.
* Total hours per day + grand total across all entries.

### 7.3 UI Components (MudBlazor Mapping)
* Date: `MudDatePicker` (default value = today).
* Project: `MudSelect<string>` with predefined items.
* Hours: `MudNumericField<decimal>` or `MudTextField<string>` with validation.
* Description: `MudTextField<string>` with `Lines` > 1.
* Save: `MudButton` (filled/contained).
* History: `MudTable` or grouped list with day headers and per-day totals.

### 7.4 Accessibility
* Label all inputs and ensure proper `aria-` attributes.
* Keyboard navigation order is logical and visible focus is present.
* Contrast and error states meet WCAG 2.2 AA.

---

## 8. GAPS & ASSUMPTIONS
Per SDD protocol, do not assume beyond this list; treat as blocking until clarified.

* `specs/` directory: [UNKNOWN] — Not present in repo at the time of writing; consider this document provisional until `specs/` is added.
* Persistence layer: [UNKNOWN] — LocalStorage, IndexedDB, or backend API not specified.
* Data model identifiers: [UNKNOWN] — Whether entries require stable IDs is not defined.
* Validation rules: [UNKNOWN] — Hours bounds, required fields, and project list finalization.
* Timezone/i18n: [UNKNOWN] — Date handling across timezones/locales not specified.
* State management: [UNKNOWN] — Preferred pattern (e.g., scoped service) not specified.
* CI workflow: [UNKNOWN] — C# build/test script flow to be defined in `scripts/`.

---

## 9. IMPLEMENTATION NOTES
* Use CSS isolation (`.razor.css`) for all components; no SCSS/Sass.
* Optimize for AOT and aggressive trimming; avoid reflection-heavy patterns.
* Prefer source generators for JSON/logging if/when persistence is introduced.
* Ensure zero compiler warnings and comprehensive tests (xUnit/bUnit/NSubstitute/Shouldly).

---

## 9.1 HOSTING & PERSISTENCE
* **Hosting:** The Blazor WebAssembly app must support static hosting on GitHub Pages.
    * Configure `base href` to the repository subpath (e.g., `/Viso.Tracker/`).
    * Publish the built `wwwroot` to `docs/` and configure Pages to deploy from `main` → `docs/`.
* **Persistence (MVP default):** IndexedDB in-browser storage to enable static hosting and offline capability with minimal bundle size.
* **Optional:** SQLite WASM (stored in OPFS/IndexedDB) for local SQL; use only if SQL is a hard requirement due to bundle size and complexity.
* **Alternative:** External backend API (ASP.NET Core + SQLite/Postgres) hosted separately; front-end remains static on GitHub Pages.

---

## 10. AI ASSISTANTS USAGE POLICY
AI assistants (GitHub Copilot, Cursor, ChatGPT, Claude) are allowed and encouraged. The developer remains fully responsible for correctness, quality, security, and compliance with this manifest and future `specs/`.

---

## 11. Commit Message Convention
All commit messages must follow the Conventional Commits specification (v1.0.0).

> [!IMPORTANT]
> Format: `<type>(<scope>): <description>`

**Types:**
- feat: A new feature
- fix: A bug fix
- docs: Documentation changes
- style: Changes that do not affect the meaning of the code
- refactor: Code change that neither fixes a bug nor adds a feature
- perf: Code change that improves performance
- test: Adding missing tests or correcting existing tests
- build: Build system or dependencies
- ci: CI/CD changes
- chore: Routine tasks, maintenance
- revert: Revert previous commit

**Rules:**
- Use lowercase for `type`.
- The description must be in the imperative, present tense (e.g., "add" not "added").
- No period at the end of the description.

> [!NOTE]
> Git hooks are configured to validate this convention via `scripts/ValidateCommitMessage.cs`. Use `dotnet scripts/SetupGitHooks.cs` or `dotnet scripts/InstallGitHooks.cs` to install hooks locally.
