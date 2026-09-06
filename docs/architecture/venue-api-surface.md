# Venue API surface, by function

Every HTTP endpoint the Venue API serves, grouped by what it does rather than by which
controller it lives in, with every place each function is built.

- **251 endpoints** across **44 controllers**, plus `Program.cs` and `Hubs/VennuHub.cs`
- resolved into **31 functions** in 6 groups
- **16 of those 31 functions are built in more than one place**

Source: `src/Vennu.Api`, branch `design/wall-planner`, 30 August 2026.
Machine-readable companion: `venue-api-surface.json` in this directory.

## What "built in more than one place" means here

Several functions are served from two separate sets of controllers: one for venue staff
under `/api/back-office/...`, and a separate one for platform support staff under
`/api/platform-operations/...`. **These are not duplicates that could be deleted.** Both are
live, both are called, and both have to keep working. A change to the function has to be
made in every place it is built, and if one is changed and the other is not, they drift apart.

Separately, most controllers also answer on a legacy prefix (`/api/admin` or
`/api/venue-admin`). That is the **same code at an older address**, not another build.
`AdministrativeCompatibilityMiddleware` stamps a `Deprecation: true` header on those and
passes the request through unchanged. See [administrative-identity.md](administrative-identity.md).

## Functions built in more than one place

| Function | Group | Places | Endpoints |
| --- | --- | --- | --- |
| [Deciding what shows on which screen](#deciding-what-shows-on-which-screen) | Screens | 5 | 18 |
| [The tap list](#the-tap-list) | Timed content | 2 | 18 |
| [Managing screens](#managing-screens) | Screens | 3 | 13 |
| [Look and feel](#look-and-feel) | Screens | 3 | 13 |
| [Till system connections](#till-system-connections) | Plumbing | 3 | 13 |
| [Menus](#menus) | The menu | 3 | 11 |
| [Registering and pairing screens](#registering-and-pairing-screens) | Screens | 5 | 11 |
| [Meal periods](#meal-periods) | Timed content | 2 | 10 |
| [Dated promotions](#dated-promotions) | Timed content | 2 | 8 |
| [Plans and paying](#plans-and-paying) | Money | 2 | 7 |
| [Video walls](#video-walls) | Screens | 2 | 6 |
| [Urgent notices](#urgent-notices) | Timed content | 2 | 6 |
| [Marking things sold out](#marking-things-sold-out) | The menu | 2 | 5 |
| [Setting up a new venue](#setting-up-a-new-venue) | Getting in | 2 | 4 |
| [Happy hour](#happy-hour) | Timed content | 2 | 4 |
| [Who am I and what can I do](#who-am-i-and-what-can-i-do) | Getting in | 3 | 3 |

---

## Getting in

Signing in, proving who you are, standing up a new venue.  
*4 functions · 22 endpoints*

### Signing in

Lets a person sign in to Vennusign — either by going through Google, Apple, or the Vennusign login page, or by asking for a one-time link emailed to them and clicking it. It also signs them back out again.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Anyone signing in | `/api/customer-auth` | 4 | — |

<details><summary>The 4 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/customer-auth/external/{provider}` | Sends a customer off to Google, Apple, or the Vennusign login page to sign in (or create an account), then brings them back to the app page they came from. |
| POST | `/api/customer-auth/email-links` | Emails the customer a one-time sign-in link so they can log in without a password. |
| POST | `/api/customer-auth/email-links/redeem` | Turns the code from the emailed sign-in link into a logged-in session and sets the session cookie. |
| DELETE | `/api/customer-auth/session` | Signs the customer out by revoking their session and clearing the cookie. |

</details>

### Proving it is really you

Lets an account holder prove they really are who they say they are, using either a passkey (fingerprint, face, or device PIN) or a six-digit code from an authenticator app, with one-time backup codes if they lose the phone. It also covers signing in with a passkey in the first place, and adding, renaming or removing the passkeys on the account.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Venue owners | `/api/customer-auth/strong` | 11 | — |

<details><summary>The 11 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/customer-auth/strong/passkeys` | Shows the customer the list of passkeys registered on their account. |
| PUT | `/api/customer-auth/strong/passkeys/{id:guid}` | Lets the customer give one of their passkeys a friendlier name. |
| DELETE | `/api/customer-auth/strong/passkeys/{id:guid}` | Lets the customer delete one of their passkeys, unless it is their only passkey and they have no verified email to fall back on. |
| POST | `/api/customer-auth/strong/passkeys/registration/options` | Starts adding a new passkey by giving the browser the challenge it needs to create one. |
| POST | `/api/customer-auth/strong/passkeys/registration/complete` | Finishes adding a new passkey by checking the browser's answer and saving the passkey to the account. |
| POST | `/api/customer-auth/strong/passkeys/assertion/options` | Starts a passkey sign-in for the given email address by giving the browser the challenge to sign. |
| POST | `/api/customer-auth/strong/passkeys/assertion/complete` | Finishes a passkey sign-in, logs the customer in with a strong (MFA-satisfied) session, and sets the session cookie. |
| POST | `/api/customer-auth/strong/totp/enrollment` | Starts setting up an authenticator app by generating the secret / QR details for the customer to scan. |
| POST | `/api/customer-auth/strong/totp/enrollment/complete` | Finishes authenticator-app setup by checking the first code and handing the customer their one-time recovery codes. |
| POST | `/api/customer-auth/strong/step-up/totp` | Upgrades the customer's current session to strong (MFA-satisfied) by checking a code from their authenticator app. |
| POST | `/api/customer-auth/strong/step-up/recovery-code` | Upgrades the customer's current session to strong by spending one of their one-time recovery codes. |

</details>

### Who am I and what can I do — **built in 3 places**

This is the check the app makes the moment someone opens it: it says who is signed in, which venue they are working in, which other venues they can switch to, and which parts of the product are switched on for them. Every screen in the back office and the support console relies on this answer to decide what to show.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Venue staff | `/api/back-office/session` | 1 | `/api/venue-admin` |
| Venue owners | `/api/customer-auth/session` | 1 | — |
| Platform staff | `/api/platform-operations/session` | 1 | `/api/admin` |

<details><summary>The 3 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/back-office/session` | Tells the signed-in back-office user who they are, which venue they are working in, which other venues they may switch to, and which features are switched on or off for them right now. |
| GET | `/api/customer-auth/session` | Tells the app who is currently signed in (id, email, display name, and how they signed in). |
| GET | `/api/platform-operations/session` | Tells the platform-operations console who is signed in and which areas of the console it may show. |

</details>

### Setting up a new venue — **built twice**

This is how a brand-new customer gets set up: they create their business record, then create their venue with its name, timezone, venue type and languages, and can check at any point how far through that setup they have got. There is also a bare venue-create endpoint that makes a venue record on its own, outside the guided signup steps.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| New customers | `/api/customer-onboarding` | 3 | — |
| Anyone (no sign-in) | `/api/venues` | 1 | — |

<details><summary>The 4 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/customer-onboarding` | Shows a new customer how far along they are in setting up their account (organization, plan, venue, first screen). |
| POST | `/api/customer-onboarding/organization` | Lets a new customer create their business (organization) record with contact details as the first onboarding step. |
| POST | `/api/customer-onboarding/venue` | Lets a new customer create their first venue (name, timezone, type, languages) once they have a plan. |
| POST | `/api/venues` | Creates a new venue record (name, timezone, type, languages) and returns its id. |

</details>

---

## Money

Plans, payment, hardware rental, what the business earns.  
*3 functions · 10 endpoints*

### Plans and paying — **built twice**

This is how a venue picks a plan and pays for it. A new customer signing up can see the plans on offer, start a free trial, or go straight to a card payment page; an existing venue owner can see what plan they are on, how much of their screen and venue allowance they are using, what other plans they could move to and what they would lose by moving, and can open Stripe to change plan or update their card.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Venue owners | `/api/back-office/billing` | 4 | `/api/venue-admin` |
| New customers | `/api/customer-onboarding` | 3 | — |

<details><summary>The 7 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/back-office/billing/presentation` | Shows the venue's billing page: its current plan and subscription status, how many screens and venues it is using against its limits, which other plans it could move to (and which features it would lose), plus any hardware-rental bundles and the venue's current hardware contract. |
| POST | `/api/back-office/billing/tier-portal-session` | Checks that the venue is allowed to move to the plan it picked, then opens the Stripe billing portal so the owner can change their plan there. |
| POST | `/api/back-office/billing/portal-session` | Opens the Stripe billing portal so the venue owner can manage their payment details and subscription. |
| POST | `/api/back-office/billing/checkout-session` | Starts a Stripe checkout so the venue owner can buy a software plan, billed monthly or yearly, after confirming the venue is eligible for that plan. |
| GET | `/api/customer-onboarding/plans` | Shows anyone the public list of subscription plans they can pick from when signing up. |
| POST | `/api/customer-onboarding/trial` | Lets a new customer start a free trial on a chosen plan for their organization. |
| POST | `/api/customer-onboarding/checkout` | Gives a new customer a Stripe payment page link to buy a chosen plan, billed monthly or annually. |

</details>

### Renting screen hardware

A venue owner can rent their screen hardware from Vennusign instead of buying it, paying for a fixed-term bundle of screens through the normal card checkout. This is the one step that starts that rental purchase.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Venue owners | `/api/back-office/billing` | 1 | `/api/venue-admin` |

<details><summary>The 1 endpoint</summary>

| Method | Route | What it does |
| --- | --- | --- |
| POST | `/api/back-office/billing/haas-checkout-session` | Starts a Stripe checkout so the venue owner can rent screen hardware on a fixed-term bundle. |

</details>

### Revenue and the platform dashboard

Lets Vennusign's own staff see what the business is earning: how much subscription money is coming in each month and each year, split by plan, plus a month-by-month history of the last two years. It also points out any price set up in the payment system that isn't tied to a plan, so nothing is being charged without a home.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Platform staff | `/api/platform-operations/dashboard/revenue` | 2 | `/api/admin` |

<details><summary>The 2 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/platform-operations/dashboard/revenue` | Shows platform staff current monthly and annual recurring revenue from Stripe, broken down by tier, and flags any Stripe prices that are not linked to a tier. |
| GET | `/api/platform-operations/dashboard/revenue/trend` | Shows platform staff how monthly recurring revenue and subscription counts have changed month by month for up to the last two years. |

</details>

---

## The menu

Everything a venue does to a menu, before and after it reaches a screen.  
*6 functions · 54 endpoints*

### Menus — **built in 3 places**

This is the shelf of menus a venue keeps: staff can see every menu they have, create a new empty one, rename it, copy one, and put an old one away (or bring it back). It also tells the menu builder the basics it needs to work — the venue's timezone, its size limits, and how many menus it is allowed versus how many it has already used.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Venue staff | `/api/back-office/content` | 7 | — |
| Venue staff | `/api/back-office/menus` | 3 | `/api/venue-admin` |
| Platform staff | `/api/platform-operations/venues/{venueId}/menus` | 1 | `/api/admin` |

<details><summary>The 11 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/back-office/content/context` | Tells the menu screens which timezone the venue is in, what its size limits are, and how many menus it has. |
| GET | `/api/back-office/content/configuration` | Tells the menu builder its settings: the biggest import file allowed, how long to wait before warning about a stuck publish, and how much history to keep. |
| GET | `/api/back-office/content/menus` | Lists every menu the venue has, with what its screens are currently showing, how many unpublished changes are waiting, and which screens it is on. |
| GET | `/api/back-office/content/menus/allowance` | Tells the venue how many menus it is allowed and how many it is already using. |
| POST | `/api/back-office/content/menus/{menuId}/duplicate` | Makes a copy of a menu that shares the same dishes but is not yet published or on any screen. |
| PUT | `/api/back-office/content/menus/{menuId}/put-away` | Puts a menu away (archives it) or brings it back onto the shelf; a menu still on screens must be taken off first. |
| GET | `/api/back-office/content/menus/{menuId}/board` | Opens a menu in the builder: the working version, what has changed, and when it was last published. |
| GET | `/api/back-office/menus` | Shows a venue user everything in their menu editor at once: all menus, their sections, the items in each section, and which extra features (happy hour, allergen badges, quick update) the venue can use. |
| POST | `/api/back-office/menus` | Lets a venue user add a new, empty menu by giving it a name. |
| PUT | `/api/back-office/menus/{menuId}` | Lets a venue user rename one of their menus. |
| GET | `/api/platform-operations/venues/{venueId}/menus` | Lets a Vennusign support person look at all of a venue's menus, sections and item groups without being able to change anything. |

</details>

### Pages and sections

Lets venue staff lay out a menu: create, rename, reorder, copy and remove the pages of a menu, and add, rename, reorder, move and remove the sections (like "Starters" or "Desserts") that sit on those pages. Deleting a page or section asks what should happen to what was on it — move it elsewhere or send it back to the library — and reordering refuses if someone else changed things first.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Venue staff | `/api/back-office/content/menus/{menuId}` | 11 | — |

<details><summary>The 11 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/back-office/content/menus/{menuId}/pages` | Lists the pages of a menu in order. |
| POST | `/api/back-office/content/menus/{menuId}/pages` | Adds a new named page to a menu. |
| PUT | `/api/back-office/content/menus/{menuId}/pages/{pageId}` | Renames a page of a menu. |
| PUT | `/api/back-office/content/menus/{menuId}/pages/order` | Puts a menu's pages into a new order, refusing if someone else changed the pages in the meantime. |
| POST | `/api/back-office/content/menus/{menuId}/pages/{pageId}/duplicate` | Makes a copy of one page inside the same menu. |
| DELETE | `/api/back-office/content/menus/{menuId}/pages/{pageId}` | Removes a page from a menu, either moving its sections to another page or deleting them, and reports how many screens lost their assignment. |
| POST | `/api/back-office/content/menus/{menuId}/sections` | Adds a new section (like 'Starters') to a menu page. |
| PUT | `/api/back-office/content/menus/{menuId}/sections/{sectionId}` | Renames a section of a menu. |
| DELETE | `/api/back-office/content/menus/{menuId}/sections/{sectionId}` | Removes a section after the user chooses whether its dishes move to another section or go back to the library. |
| POST | `/api/back-office/content/menus/{menuId}/sections/{sectionId}/page` | Moves a whole section, with its dishes, to a different page of the same menu. |
| PUT | `/api/back-office/content/menus/{menuId}/sections/order` | Puts a menu's sections into a new order, refusing if someone else changed them in the meantime. |

</details>

### Dishes and the item library

Lets venue staff build up the actual dishes on a menu — add a dish to a section (picking one from the venue's saved dish library or typing a brand new one), edit its name, description or price, drag it between sections, reorder it, and take it off a page without losing it from the library. The dish library itself is searchable, and it tells you which menus a dish is already on.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Venue staff | `/api/back-office/content` | 7 | — |

<details><summary>The 7 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| PUT | `/api/back-office/content/menus/{menuId}/sections/{sectionId}/items/order` | Puts the dishes in a section into a new order, refusing if the section changed in the meantime. |
| PUT | `/api/back-office/content/menus/{menuId}/items/{itemId}/placement` | Drags a dish from one section to another on the same page, sending both sections' orders so nothing moves if the page changed. |
| POST | `/api/back-office/content/menus/{menuId}/sections/{sectionId}/items` | Adds a dish to a section, either one already in the venue's library or a brand-new one typed in with an optional price; if it is already on the menu it says where. |
| DELETE | `/api/back-office/content/menus/{menuId}/pages/{pageId}/items/{itemId}` | Takes a dish off a menu page while keeping it in the venue's library for later. |
| PUT | `/api/back-office/content/menus/{menuId}/pages/{pageId}/items/{itemId}/transition` | Moves a dish into a section on a page (used by undo/redo), checking the section still looks the way it did before applying. |
| PUT | `/api/back-office/content/items/{itemId}` | Edits a dish's name, description, price or listed status; name and description change everywhere, price changes only for this placement unless 'everywhere' is chosen. |
| GET | `/api/back-office/content/items` | Searches the venue's whole dish library by name, saying which menus each dish is already on. |

</details>

### Marking things sold out — **built twice**

Lets venue staff flag a dish as sold out — or put it back on sale — and the change shows on every screen straight away, with no need to republish the menu. Staff can also see what is currently marked sold out, who did it and when, and put everything back on sale in one go.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Venue staff | `/api/back-office/content` | 4 | — |
| Venue staff | `/api/back-office/menus/{menuId}/sections/{sectionId}/items/{itemId}` | 1 | `/api/venue-admin` |

<details><summary>The 5 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/back-office/content/quick-update` | Gives staff one screen showing every menu that is live on a screen, which dishes are sold out, and which screens are showing what, so they can quickly mark things unavailable. |
| PUT | `/api/back-office/content/items/{itemId}/availability` | Marks a dish as sold out (or back on) and it disappears from or reappears on every screen immediately, without a publish. |
| POST | `/api/back-office/content/availability/restore-all` | Puts every sold-out dish back on sale in one go, across all screens. |
| GET | `/api/back-office/content/availability` | Lists which dishes are currently marked sold out, who changed them and when. |
| PUT | `/api/back-office/menus/{menuId}/sections/{sectionId}/items/{itemId}/quick-availability` | Lets a venue user mark a menu item as sold out or back in stock from the Home page, and pushes that change to the screens showing it right away. |

</details>

### Publishing and history

Lets venue staff hold their menu edits as unpublished changes, see what is waiting, then send them all to the screens in one go — or throw them away. It also keeps a record of past publishes so a menu can be put back to how it looked before.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Venue staff | `/api/back-office/content/menus/{menuId}` | 6 | — |

<details><summary>The 7 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/back-office/content/menus/{menuId}/published-board` | Shows the version of one menu that its screens are showing right now. |
| GET | `/api/back-office/content/menus/{menuId}/draft` | Shows what has changed in a menu since it was last sent to the screens. |
| DELETE | `/api/back-office/content/menus/{menuId}/draft` | Throws away all unpublished edits, putting the menu back to what the screens are showing. |
| POST | `/api/back-office/content/menus/{menuId}/publish` | Sends all of a menu's waiting changes to its screens, all at once or not at all. |
| GET | `/api/back-office/content/menus/{menuId}/history` | Shows the record of publishes and other events for a menu, limited to the venue's retention depth. |
| GET | `/api/back-office/content/menus/{menuId}/pages/{pageId}/history` | Shows the history of one page of a menu. |
| POST | `/api/back-office/content/menus/{menuId}/go-back-to/{version}` | Restores a menu to an earlier published version as a set of unpublished changes that still need a publish to reach the screens. |

</details>

### Importing a menu from pasted text

Someone at a venue can paste the text of their menu and have the system turn it into a real menu: it reads the lines, spots which dishes it already knows about, and asks the person to settle anything it isn't sure of. They can then either create a brand-new menu from the result or overwrite an existing one, with a saved copy kept so the old menu can be put back.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Venue staff | `/api/back-office/menu-imports` | 14 | — |

<details><summary>The 13 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| POST | `/api/back-office/menu-imports` | Lets a venue user paste the text of a menu to begin an import; the system reads the lines, matches them against items the venue already has, and comes back with the questions it needs answered. |
| GET | `/api/back-office/menu-imports` | Shows a venue user the imports they started but have not finished, so they can pick one up again from the Menus home page. |
| DELETE | `/api/back-office/menu-imports/{sessionId}` | Lets a venue user throw away an unfinished import instead of waiting for it to expire. |
| GET | `/api/back-office/menu-imports/{sessionId}` | Shows a venue user the current state of one import: the parsed lines, the review questions, and the answers given so far. |
| PUT | `/api/back-office/menu-imports/{sessionId}/answers/{questionKey}` | Lets a venue user answer one review question about a pasted line, for example choosing whether it is a new item or the same as an item the venue already has. |
| POST | `/api/back-office/menu-imports/{sessionId}/accept-safe-matches` | Lets a venue user accept in one click every line the system is confident is the same as an existing item, instead of answering those questions one by one. |
| POST | `/api/back-office/menu-imports/{sessionId}/lines/{lineNumber}/promote-to-section` | Lets a venue user tell the import that a pasted line is a section heading (like 'Desserts') rather than a dish, and re-reads the paste with that correction. |
| DELETE | `/api/back-office/menu-imports/{sessionId}/lines/{lineNumber}/section-promotion` | Lets a venue user undo marking a pasted line as a section heading, so it goes back to being treated as a dish. |
| PUT | `/api/back-office/menu-imports/{sessionId}/destination/create` | Lets a venue user say the import should become a brand-new menu, and what to call it. |
| POST | `/api/back-office/menu-imports/{sessionId}/destination/create/confirm` | Lets a venue user finish the import by actually creating the new menu with the reviewed items. |
| PUT | `/api/back-office/menu-imports/{sessionId}/destination/replace` | Lets a venue user choose an existing menu that the import will overwrite, and shows a preview of what would change. |
| POST | `/api/back-office/menu-imports/{sessionId}/destination/replace/confirm` | Lets a venue user finish the import by replacing the chosen existing menu's contents with the reviewed items, keeping a saved copy of the old version. |
| POST | `/api/back-office/menu-imports/replacement-snapshots/{snapshotId}/restore` | Lets a venue user put a menu back the way it was before an import replaced it, using the saved copy. |

</details>

---

## Screens

The screens on walls: registering them, running them, choosing what they show.  
*5 functions · 61 endpoints*

### Registering and pairing screens — **built in 5 places**

This is how a physical display gets turned into a screen the venue can send menus to. A device is either plugged in and shows a short code that someone types in to link it to the venue, or it is set up in advance before it ships so it connects itself the moment it is powered on.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Venue staff | `/api/back-office/venues/{venueId}/screens` | 2 | `/api/venue-admin` |
| Venue staff | `/api/back-office/screens/pairing` | 1 | `/api/venue-admin` |
| Platform staff | `/api/platform-operations/venues/{venueId}/screens` | 2 | `/api/admin` |
| Screens + platform staff | `/api/screens` | 5 | — |
| New customers | `/api/customer-onboarding/first-screen` | 1 | — |

<details><summary>The 11 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| POST | `/api/back-office/venues/{venueId}/screens/pre-registrations` | Registers a screen in advance for a device that will be shipped to the venue, handing back a one-time setup token the device uses to connect itself when it arrives. |
| POST | `/api/back-office/venues/{venueId}/screens` | Adds a new screen to the venue with default display settings, ready to be paired with a device later. |
| POST | `/api/back-office/screens/pairing/{code}/claim` | Links a new device to the signed-in venue by entering the pairing code the device is showing on its screen. |
| POST | `/api/customer-onboarding/first-screen` | Lets a new customer link their first display to their venue by typing the six-digit code shown on the screen. |
| POST | `/api/screens/pre-registration/claim` | Lets a screen that was pre-registered by platform staff (hardware-as-a-service) wake up and claim its identity using the one-time bootstrap token shipped with it. |
| POST | `/api/screens` | Lets a brand-new display device register itself and receive a screen id and screen key before it has been linked to any venue. |
| POST | `/api/screens/pairing-code` | Lets a display device ask for a short pairing code (valid 10 minutes) that a person can type into the operations console to link the screen to a venue. |
| GET | `/api/screens/pairing/{code}/status` | Lets a waiting display device poll whether its pairing code has been claimed yet, and if so which screen it is now linked as. |
| POST | `/api/screens/pairing/{code}/claim` | Lets platform operations staff type a screen's pairing code and attach that screen to a chosen venue. |
| POST | `/api/platform-operations/venues/{venueId}/screens/pre-registrations` | Lets platform staff set up a screen for a venue ahead of time and get a one-off setup code so the physical device can claim it when it is plugged in. |
| POST | `/api/platform-operations/venues/{venueId}/screens` | Adds a new, not-yet-paired screen to a venue with a name and optional location. |

</details>

### Managing screens — **built in 3 places**

This is the day-to-day upkeep of the TV screens in a venue: seeing the list of screens and how each one is doing, naming them and setting where they hang and how they lay content out, pushing fresh content to one screen or all of them at once, and dealing with the physical boxes — restarting a screen that has gone odd, disconnecting a device, swapping a broken one for a new one that inherits everything, and shelving a screen the venue no longer uses.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Venue staff | `/api/back-office/venues/{venueId}/screens` | 7 | `/api/venue-admin` |
| Venue staff | `/api/back-office/screens/pairing` | 2 | `/api/venue-admin` |
| Platform staff | `/api/platform-operations/venues/{venueId}/screens` | 4 | `/api/admin` |

<details><summary>The 13 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/back-office/venues/{venueId}/screens` | Shows the venue's list of screens with each one's status, settings, and latest content-delivery information. |
| PUT | `/api/back-office/venues/{venueId}/screens/{screenId}` | Lets venue staff rename a screen and change its location and display settings such as layout, photo grid density, split ratio, and how long the hero image stays up. |
| POST | `/api/back-office/venues/{venueId}/screens/{screenId}/push` | Immediately sends the venue's current content to one screen instead of waiting for its normal refresh. |
| PUT | `/api/back-office/venues/{venueId}/screens/{screenId}/lifecycle` | Archives a screen the venue no longer uses, or brings an archived screen back into service. |
| POST | `/api/back-office/venues/{venueId}/screens/{screenId}/reset` | Forces a misbehaving screen to start over by marking it offline so it reconnects and fetches everything fresh. |
| DELETE | `/api/back-office/venues/{venueId}/screens/{screenId}/pairing` | Disconnects a physical device from the venue's screen so the device no longer shows this venue's content and can be paired elsewhere. |
| POST | `/api/back-office/venues/{venueId}/screens/push-all` | Sends the latest content to every active screen in the venue at once and reports how many screens were reached. |
| POST | `/api/back-office/screens/pairing/replacement/preview` | Checks whether a new device can take over an existing screen's job and shows what would carry over - name, settings, history, and video-wall spot - before anything changes. |
| POST | `/api/back-office/screens/pairing/replacement` | Swaps a broken screen's device for a new one so the screen keeps its name, settings, history, and place in any video wall. |
| GET | `/api/platform-operations/venues/{venueId}/screens` | Lists all the screens that belong to a venue, with their name, location, status and display settings. |
| PUT | `/api/platform-operations/venues/{venueId}/screens/{screenId}` | Renames a screen or changes where it is and how it lays out content (photo grid density, layout, split ratio, how long a hero image stays up). |
| POST | `/api/platform-operations/venues/{venueId}/screens/{screenId}/push` | Forces one screen to refresh and pick up the latest content right now. |
| POST | `/api/platform-operations/venues/{venueId}/screens/push-all` | Forces every active screen in a venue to refresh and pick up the latest content, and reports how many screens were told to. |

</details>

### Deciding what shows on which screen — **built in 5 places**

This is how a venue decides what each screen in the room actually shows: putting a particular menu page onto a particular screen, taking it back off, seeing at a glance what every screen is currently showing and who put it there, and building a rotation of slides that a screen cycles through with its own timings and day/time windows. It also includes a preview that shows how many menu items will fit on a screen of a given size and which ones would spill off the bottom.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Venue staff | `/api/back-office/content` | 6 | — |
| Venue staff | `/api/back-office/venues/{venueId}/screens/{screenId}/playlist` | 5 | `/api/venue-admin` |
| Venue staff | `/api/back-office/venues/{venueId}/screens/overflow` | 1 | `/api/venue-admin` |
| Platform staff | `/api/platform-operations/venues/{venueId}/screens/{screenId}/playlist` | 5 | `/api/admin` |
| Platform staff | `/api/platform-operations/venues/{venueId}/screens/overflow` | 1 | `/api/admin` |

<details><summary>The 18 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/back-office/content/assignments` | Lists which menu page each screen has been assigned, and who assigned it. |
| GET | `/api/back-office/content/screens/showing` | Lists every screen in the venue with what it is showing, its online status, size and the publish that put it there. |
| PUT | `/api/back-office/content/screens/{screenId}/menu` | Puts a menu page on a screen, either replacing what is there or adding it to the screen's rotation. |
| DELETE | `/api/back-office/content/screens/{screenId}/menus/{menuId}/pages/{pageId}` | Takes one menu page off one screen. |
| PUT | `/api/back-office/content/menus/{menuId}/screens` | Saves a whole set of screen changes for a menu at once — which pages go on which screens, replacing, rotating or removing. |
| DELETE | `/api/back-office/content/menus/{menuId}/screens` | Queues taking a menu off all its screens; it happens on the next publish rather than immediately. |
| GET | `/api/back-office/venues/{venueId}/screens/overflow?capacity={n}` | Previews which menu items will fit on a screen that shows a set number of items, and which ones will spill off the bottom. |
| GET | `/api/back-office/venues/{venueId}/screens/{screenId}/playlist` | Shows the ordered list of slides a screen rotates through. |
| POST | `/api/back-office/venues/{venueId}/screens/{screenId}/playlist` | Adds a new slide to a screen's rotation, with its content, how long it stays up, and an optional daily time window and days of the week. |
| PUT | `/api/back-office/venues/{venueId}/screens/{screenId}/playlist/{slideId}` | Edits an existing slide - its content, timing, schedule, or whether it is switched on. |
| PUT | `/api/back-office/venues/{venueId}/screens/{screenId}/playlist/order` | Rearranges the order the slides play in. |
| DELETE | `/api/back-office/venues/{venueId}/screens/{screenId}/playlist/{slideId}` | Removes a slide from the screen's rotation. |
| GET | `/api/platform-operations/venues/{venueId}/screens/{screenId}/playlist` | Shows a support person the slides that rotate on one particular screen at a venue. |
| POST | `/api/platform-operations/venues/{venueId}/screens/{screenId}/playlist` | Lets a support person add a slide to a screen's rotation. |
| PUT | `/api/platform-operations/venues/{venueId}/screens/{screenId}/playlist/{slideId}` | Lets a support person edit one slide in a screen's rotation. |
| PUT | `/api/platform-operations/venues/{venueId}/screens/{screenId}/playlist/order` | Lets a support person change the order slides play in on a screen. |
| DELETE | `/api/platform-operations/venues/{venueId}/screens/{screenId}/playlist/{slideId}` | Lets a support person remove a slide from a screen's rotation. |
| GET | `/api/platform-operations/venues/{venueId}/screens/overflow?capacity=` | Shows which menu items would fit on a screen that holds a set number of items (4, 6, 8 or 9) and which would be cut off, using the venue's oldest active menu. |

</details>

### Video walls — **built twice**

Lets someone join two, three or four screens together into a named group that behaves as one big display, choose the arrangement (side by side, a row of three, or a two-by-two block) and say which screen sits in each position. The same group can later be broken up so every screen goes back to showing its own content.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Venue staff | `/api/back-office/venues/{venueId}/screens/video-walls` | 3 | `/api/venue-admin` |
| Platform staff | `/api/platform-operations/venues/{venueId}/screens/video-walls` | 3 | `/api/admin` |

<details><summary>The 6 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/back-office/venues/{venueId}/screens/video-walls` | Lists the venue's video walls - named groups of screens that act together as one big display - and which screen sits in each position. |
| PUT | `/api/back-office/venues/{venueId}/screens/video-walls` | Creates or rearranges a video wall by naming it, picking a layout of 2, 3, or 4 screens, and assigning which screen goes where. |
| DELETE | `/api/back-office/venues/{venueId}/screens/video-walls/{name}` | Breaks up a video wall so each of its screens goes back to showing content on its own. |
| GET | `/api/platform-operations/venues/{venueId}/screens/video-walls` | Lists the groups of screens that have been joined together to act as one big video wall. |
| PUT | `/api/platform-operations/venues/{venueId}/screens/video-walls` | Creates or replaces a named video wall by joining 2, 3 or 4 screens together in a 2x1, 3x1 or 2x2 arrangement. |
| DELETE | `/api/platform-operations/venues/{venueId}/screens/video-walls/{name}` | Breaks up a named video wall so its screens go back to working on their own. |

</details>

### Look and feel — **built in 3 places**

Sets how a venue's screens look — background and accent colours, fonts, title and glow colours, and section colours — either by picking one of the ready-made designs (Bar Classic, Violet Lounge, Hot Summer, Ocean Dive, Rose Gold) or by choosing every colour and font by hand. It can also be reset back to the default look at any time.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Venue staff | `/api/back-office/venues/{venueId}/theme` | 6 | `/api/venue-admin` |
| Platform staff | `/api/platform-operations/venues/{venueId}/theme` | 6 | `/api/admin` |
| Venue staff | `/api/back-office/content` | 1 | — |

<details><summary>The 13 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/back-office/content/menu-themes` | Lists the menu looks (themes) the venue could choose; currently always empty. |
| GET | `/api/back-office/venues/{venueId}/theme/presets` | Lists the ready-made theme designs a venue can pick from. |
| GET | `/api/back-office/venues/{venueId}/theme` | Shows the venue's current look - its colors and fonts as displayed on screens. |
| PUT | `/api/back-office/venues/{venueId}/theme` | Changes the venue's basic look: background color, accent color, and font. |
| PUT | `/api/back-office/venues/{venueId}/theme/advanced` | Gives finer control over the board's appearance: title and glow colors, board background, up to four section colors, glow strength, and separate title and item fonts. |
| PUT | `/api/back-office/venues/{venueId}/theme/presets/{presetKey}` | Applies one of the ready-made designs to the venue in a single step. |
| DELETE | `/api/back-office/venues/{venueId}/theme` | Throws away the venue's customizations and puts the screens back on the default look. |
| GET | `/api/platform-operations/venues/{venueId}/theme/presets` | Lists the built-in ready-made looks (Bar Classic, Violet Lounge, Hot Summer, Ocean Dive, Rose Gold) a venue can pick for its screens. |
| GET | `/api/platform-operations/venues/{venueId}/theme` | Shows the colours and fonts a venue's screens currently use. |
| PUT | `/api/platform-operations/venues/{venueId}/theme` | Sets a venue's basic look: background colour, accent colour and font (Inter, Georgia or Arial). |
| PUT | `/api/platform-operations/venues/{venueId}/theme/advanced` | Sets a venue's detailed custom look: title colour, glow colour and strength, board background, section colours, and title/item fonts. |
| PUT | `/api/platform-operations/venues/{venueId}/theme/presets/{presetKey}` | Applies one of the ready-made looks to a venue's screens by its key. |
| DELETE | `/api/platform-operations/venues/{venueId}/theme` | Wipes a venue's custom look and puts its screens back to the default colours and fonts. |

</details>

---

## Timed content

Content that switches itself on and off by the clock or the calendar.  
*5 functions · 46 endpoints*

### Happy hour — **built twice**

Happy hour lets a venue set the times and days its happy hour runs, switch it on or off, and see whether it is running right now in the venue's own local time. There is also an override to force happy hour on or off no matter what the schedule says.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Venue staff | `/api/back-office/venues/{venueId}/happy-hour` | 2 | `/api/venue-admin` |
| Platform staff | `/api/platform-operations/venues/{venueId}/happy-hour` | 2 | `/api/admin` |

<details><summary>The 4 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/back-office/venues/{venueId}/happy-hour` | Shows a venue owner their happy hour schedule and whether happy hour is running right now in the venue's local time. |
| PUT | `/api/back-office/venues/{venueId}/happy-hour` | Lets a venue owner set the daily happy hour start and end time, which days it runs, whether it is on, and whether to force it on or off regardless of the clock. |
| GET | `/api/platform-operations/venues/{venueId}/happy-hour` | Shows a support person the venue's happy hour schedule and whether happy hour is on right now. |
| PUT | `/api/platform-operations/venues/{venueId}/happy-hour` | Lets a support person set the venue's happy hour times and days, turn it on or off, or force it on or off regardless of the schedule. |

</details>

### Meal periods — **built twice**

Meal periods let a venue set up its named parts of the day — breakfast, lunch, dinner and so on — each with its own start and end time, the days it runs, and optionally the layout, the menu it filters to, and the look it uses while it is on. The venue can add, edit, delete and reorder them, and the list warns about times that overlap and shows which period is running now and which comes next in the venue's own local time.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Venue staff | `/api/back-office/venues/{venueId}/meal-periods` | 5 | `/api/venue-admin` |
| Platform staff | `/api/platform-operations/venues/{venueId}/meal-periods` | 5 | `/api/admin` |

<details><summary>The 10 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/back-office/venues/{venueId}/meal-periods` | Shows a venue owner their meal periods (breakfast, lunch, dinner and so on), flags any that overlap, and says which one is on now and which comes next in venue local time. |
| POST | `/api/back-office/venues/{venueId}/meal-periods` | Lets a venue owner add a new meal period with its time window, active days, and an optional layout, menu filter and theme preset to show during it. |
| PUT | `/api/back-office/venues/{venueId}/meal-periods/{mealPeriodId}` | Lets a venue owner change an existing meal period's name, times, days, on/off state or what it shows. |
| DELETE | `/api/back-office/venues/{venueId}/meal-periods/{mealPeriodId}` | Lets a venue owner permanently remove a meal period. |
| PUT | `/api/back-office/venues/{venueId}/meal-periods/order` | Lets a venue owner drag meal periods into a new display order. |
| GET | `/api/platform-operations/venues/{venueId}/meal-periods` | Shows a support person the venue's meal periods (breakfast, lunch, dinner, etc.), any overlapping ones, which is active right now and which comes next. |
| POST | `/api/platform-operations/venues/{venueId}/meal-periods` | Lets a support person add a new meal period to a venue with its times, days, layout, menu filter and theme. |
| PUT | `/api/platform-operations/venues/{venueId}/meal-periods/{mealPeriodId}` | Lets a support person change an existing meal period's name, times, days, layout, menu filter or theme. |
| DELETE | `/api/platform-operations/venues/{venueId}/meal-periods/{mealPeriodId}` | Lets a support person remove a meal period from a venue. |
| PUT | `/api/platform-operations/venues/{venueId}/meal-periods/order` | Lets a support person change the order the venue's meal periods are listed in. |

</details>

### Dated promotions — **built twice**

Lets someone set up a special message that runs on the venue's screens between a start date and an end date — a holiday special, for example — with its own wording and layout, and a priority that decides which one wins if several are running at once. Promotions can be edited or switched off at any time, and the screens pick up the change straight away; switching one off keeps the record rather than deleting it.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Venue staff | `/api/back-office/venues/{venueId}/date-range-promotions` | 4 | `/api/venue-admin` |
| Platform staff | `/api/platform-operations/venues/{venueId}/date-range-promotions` | 4 | `/api/admin` |

<details><summary>The 8 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/back-office/venues/{venueId}/date-range-promotions` | Shows a venue owner every date-based promotion they have set up for their venue, including ones that have been switched off. |
| POST | `/api/back-office/venues/{venueId}/date-range-promotions` | Lets a venue owner add a new promotion that runs between two dates (e.g. a holiday special) with an optional title, message and screen layout, then tells the venue's screens something changed. |
| PUT | `/api/back-office/venues/{venueId}/date-range-promotions/{promotionId}` | Lets a venue owner change an existing date-based promotion's dates, wording, priority or on/off state, and notifies the venue's screens. |
| DELETE | `/api/back-office/venues/{venueId}/date-range-promotions/{promotionId}` | Lets a venue owner switch off a promotion so it stops appearing on screens; the record is kept rather than removed. |
| GET | `/api/platform-operations/venues/{venueId}/date-range-promotions` | Shows a support person the venue's date-based promotions (e.g. a holiday special running between two dates). |
| POST | `/api/platform-operations/venues/{venueId}/date-range-promotions` | Lets a support person add a promotion that shows on the venue's screens between a start and end date, and pushes the change to screens immediately. |
| PUT | `/api/platform-operations/venues/{venueId}/date-range-promotions/{promotionId}` | Lets a support person edit an existing promotion's dates, wording, priority or on/off state, and pushes the change to screens. |
| DELETE | `/api/platform-operations/venues/{venueId}/date-range-promotions/{promotionId}` | Lets a support person retire a promotion so it no longer shows, and pushes the change to screens. |

</details>

### Urgent notices — **built twice**

Puts an urgent message — with an optional picture — straight onto one screen or every screen in a venue for a set number of minutes, so it takes over what people are watching. The message can be taken down early, and there is a running list of every urgent message ever sent, including the ones that have expired or been pulled.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Venue staff | `/api/back-office/venues/{venueId}/emergency-broadcasts` | 3 | `/api/venue-admin` |
| Platform staff | `/api/platform-operations/venues/{venueId}/emergency-broadcasts` | 3 | `/api/admin` |

<details><summary>The 6 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/back-office/venues/{venueId}/emergency-broadcasts` | Shows a venue owner all emergency messages ever sent to their screens, including expired and cancelled ones. |
| POST | `/api/back-office/venues/{venueId}/emergency-broadcasts` | Lets a venue owner push an urgent message (with optional picture link) onto one screen or every screen in the venue for a set number of minutes, and pushes it to those screens immediately. |
| DELETE | `/api/back-office/venues/{venueId}/emergency-broadcasts/{broadcastId}` | Lets a venue owner take an emergency message down from screens before its time is up. |
| GET | `/api/platform-operations/venues/{venueId}/emergency-broadcasts` | Shows a support person the emergency messages that have been sent to a venue's screens. |
| POST | `/api/platform-operations/venues/{venueId}/emergency-broadcasts` | Lets a support person put an urgent message (optionally with an image) on all of a venue's screens, or on one chosen screen, for a set number of minutes, and pushes it out immediately. |
| DELETE | `/api/platform-operations/venues/{venueId}/emergency-broadcasts/{broadcastId}` | Lets a support person take an emergency message off the screens early. |

</details>

### The tap list — **built twice**

The tap list is where a venue keeps the drinks currently on tap — grouped into categories like IPAs or Lagers, each drink with its style, strength, bitterness, price and colours, and marked available, sold out or coming soon. Categories and drinks can be added, edited, removed and dragged into the order they appear, and every change is pushed straight out to the venue's screens.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Venue staff | `/api/back-office/venues/{venueId}/tap-list` | 9 | `/api/venue-admin` |
| Platform staff | `/api/platform-operations/venues/{venueId}/tap-list` | 9 | `/api/admin` |

<details><summary>The 18 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/back-office/venues/{venueId}/tap-list` | Shows a venue owner their full beer/tap list: the categories and every item on tap. |
| POST | `/api/back-office/venues/{venueId}/tap-list/categories` | Lets a venue owner add a new tap list category (e.g. IPAs) with an optional shared price, and refreshes the venue's screens. |
| PUT | `/api/back-office/venues/{venueId}/tap-list/categories/{categoryId}` | Lets a venue owner rename a tap list category, change its price, or hide it, and refreshes the venue's screens. |
| DELETE | `/api/back-office/venues/{venueId}/tap-list/categories/{categoryId}` | Lets a venue owner remove an empty tap list category; if any taps still belong to it the request is refused with a 'category is in use' message. |
| POST | `/api/back-office/venues/{venueId}/tap-list/items` | Lets a venue owner add a beer or drink to the tap list with its style, strength, bitterness, price, colours and availability, and refreshes the venue's screens. |
| PUT | `/api/back-office/venues/{venueId}/tap-list/items/{itemId}` | Lets a venue owner edit a tap list item, move it to another category, or mark it sold out / coming soon, and refreshes the venue's screens. |
| DELETE | `/api/back-office/venues/{venueId}/tap-list/items/{itemId}` | Lets a venue owner permanently remove a drink from the tap list and refreshes the venue's screens. |
| PUT | `/api/back-office/venues/{venueId}/tap-list/categories/order` | Lets a venue owner drag tap list categories into a new order and refreshes the venue's screens. |
| PUT | `/api/back-office/venues/{venueId}/tap-list/items/order` | Lets a venue owner drag tap list items into a new order and refreshes the venue's screens. |
| GET | `/api/platform-operations/venues/{venueId}/tap-list` | Shows a support person the venue's beer tap list: the categories and all the beers in them. |
| POST | `/api/platform-operations/venues/{venueId}/tap-list/categories` | Lets a support person add a tap list category (e.g. IPAs) with an optional shared price, and pushes it to screens. |
| PUT | `/api/platform-operations/venues/{venueId}/tap-list/categories/{categoryId}` | Lets a support person rename a tap list category, change its price or switch it on or off, and pushes it to screens. |
| DELETE | `/api/platform-operations/venues/{venueId}/tap-list/categories/{categoryId}` | Lets a support person remove a tap list category, as long as no beers are still filed under it. |
| POST | `/api/platform-operations/venues/{venueId}/tap-list/items` | Lets a support person add a beer to the tap list with its style, strength, price, colours and availability, and pushes it to screens. |
| PUT | `/api/platform-operations/venues/{venueId}/tap-list/items/{itemId}` | Lets a support person edit a beer on the tap list (details, price, sold out, coming soon), and pushes it to screens. |
| DELETE | `/api/platform-operations/venues/{venueId}/tap-list/items/{itemId}` | Lets a support person remove a beer from the tap list and pushes the change to screens. |
| PUT | `/api/platform-operations/venues/{venueId}/tap-list/categories/order` | Lets a support person change the order tap list categories appear on screen. |
| PUT | `/api/platform-operations/venues/{venueId}/tap-list/items/order` | Lets a support person change the order beers appear on screen. |

</details>

---

## Plumbing

Other systems, what a screen itself calls, and running the platform.  
*8 functions · 58 endpoints*

### Till system connections — **built in 3 places**

Lets a venue link its till system — Clover, Square or Toast — to Vennusign, check whether that link is healthy and when it last synced, and pull the venue's items and categories across so they can be put on menus. The venue can also unlink the till at any time.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Venue staff | `/api/back-office/pos/clover` | 5 | `/api/venue-admin` |
| Venue staff | `/api/back-office/pos/square` | 5 | `/api/venue-admin` |
| Venue staff | `/api/back-office/pos/toast` | 3 | `/api/venue-admin` |

<details><summary>The 13 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| POST | `/api/back-office/pos/clover/connect` | Gives the venue's back-office user the Clover sign-in link to start linking their Clover account to this venue. |
| GET | `/api/back-office/pos/clover/status` | Shows the venue whether Clover is connected, when it last synced, how many syncs have failed in a row, and reminds them that the Clover webhook still has to be registered by hand in Clover's developer dashboard. |
| DELETE | `/api/back-office/pos/clover/connection` | Lets the venue unlink its Clover account from Vennusign. |
| POST | `/api/back-office/pos/clover/catalog/import` | Pulls the venue's items and categories from Clover into Vennusign so they can be used on menus. |
| GET | `/api/back-office/pos/clover/callback` | The page Clover sends the user back to after they approve access; it finishes the link-up and then bounces the user to the back office with a connected, denied, or error message. |
| POST | `/api/back-office/pos/square/connect` | Gives the venue's back-office user the Square sign-in link to start linking their Square account to this venue. |
| GET | `/api/back-office/pos/square/status` | Shows the venue whether Square is connected and when the connection was last updated. |
| DELETE | `/api/back-office/pos/square/connection` | Lets the venue unlink its Square account from Vennusign. |
| POST | `/api/back-office/pos/square/catalog/import` | Pulls the venue's items and categories from Square into Vennusign so they can be used on menus. |
| GET | `/api/back-office/pos/square/callback` | The page Square sends the user back to after they approve access; it finishes the link-up and then bounces the user to the back office with a connected, denied, or error message. |
| PUT | `/api/back-office/pos/toast/connection` | Lets the venue link Toast by pasting in their Toast restaurant ID and access token, which Vennusign stores encrypted. |
| GET | `/api/back-office/pos/toast/status` | Shows the venue whether Toast is linked, whether the background sync is healthy, retrying, pending, or needs re-authorising, and reminds them the Toast webhook must be registered by hand in Toast's developer portal. |
| POST | `/api/back-office/pos/toast/catalog/import` | Pulls the venue's menu items from Toast into Vennusign so they can be used on menus. |

</details>

### What a screen on the wall calls

This is how a TV on the venue wall gets what it shows and stays in step: it asks for its current content — layout, colours, menu pages, any promotion, happy hour, playlist or emergency message — checks in to say it is alive, and confirms which version it actually put on the glass. It also holds an open live connection so a publish reaches it the moment it happens, and anyone standing at the venue can open a no-sign-in health page for a single screen.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Screens | `/api/display/{screenId}` | 4 | — |
| Screens | `/hubs/vennusign` | 7 | `/hubs/vennu` |

<details><summary>The 12 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/display/{screenId}/content` | Gives a display screen everything it should show right now: its layout, colours and fonts, any active promotion, emergency message, playlist or happy hour, and the menu pages it has been assigned, already split up to fit this screen and its neighbours in a video wall. |
| POST | `/api/display/{screenId}/heartbeat` | Lets a display screen check in and report that it is online, which platform and app version it runs, so the back office can tell whether it is alive; the first online check-in also ticks off the customer's go-live onboarding step. |
| POST | `/api/display/{screenId}/content-receipts` | Lets a display screen confirm which version of its content it received and applied (or report that applying it failed), so the back office can see whether a publish actually reached the screen. |
| GET | `/api/display/{screenId}/diagnostics` | Shows a technician at the venue a health summary for one screen without signing in: when it last checked in, whether it looks stale, which content version it has, and whether it is the customer's first onboarding screen. |
| HUB | `/hubs/vennusign` | The live connection display screens use to be told instantly when their content changes. |
| HUB | `/hubs/vennu` | An older address for the same live connection, kept so old display builds still connect. |
| HUB | `JoinScreen(screenId)` | Lets a display screen subscribe to live updates meant for that one screen. |
| HUB | `LeaveScreen(screenId)` | Lets a display screen stop receiving live updates for that screen. |
| HUB | `JoinVenue(venueId)` | Would let a client receive every live update for a whole venue, including unpublished draft edits. |
| HUB | `LeaveVenue(venueId)` | Stops receiving whole-venue live updates. |
| HUB | `JoinVideoWall(wallId, position)` | Lets a screen that is part of a multi-screen video wall subscribe to updates for that wall. |
| HUB | `LeaveVideoWall(wallId)` | Stops receiving video-wall updates. |

</details>

### Incoming calls from Stripe and tills

This is how outside systems tell Vennusign that something has changed, without anyone logging in. Stripe reports subscription and payment changes so a venue's plan and hardware rental stay in step with billing, and a venue's till system (Square, Toast or Clover) reports menu or stock changes so the boards can be updated in the background.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Stripe | `/api/webhooks/stripe` | 1 | — |
| Till systems | `/api/webhooks/pos/{provider}` | 1 | — |

<details><summary>The 2 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| POST | `/api/webhooks/pos/{provider}` | Lets a connected point-of-sale system (Square, Toast or Clover) tell Vennusign that something changed in the venue's menu or stock, so Vennusign can queue it up and update the venue's boards in the background. |
| POST | `/api/webhooks/stripe` | Lets Stripe tell Vennusign when a customer's subscription is created, changed, cancelled or paid for, so the venue's plan and any hardware-rental contract are kept in step with billing. |

</details>

### Managing venues across the platform

Lets Vennusign's own support staff look after every venue on the platform from one place: create a new venue, browse and filter the whole list, open a single venue to see its details, plan, screens and features, move it to a different plan, and hand-unlock or block an individual feature for it. They can also watch recent sign-ups and see how far each new customer got.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Platform staff | `/api/platform-operations/venues` | 6 | `/api/admin` |
| Platform staff | `/api/platform-operations/onboarding` | 1 | `/api/admin` |

<details><summary>The 7 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| POST | `/api/platform-operations/venues` | Lets platform staff create a new venue, which automatically starts it on a Starter-tier free trial. |
| PUT | `/api/platform-operations/venues/{venueId}/tier` | Lets platform staff move a venue to a different subscription tier, updating the venue's Stripe subscription and rolling back if anything fails. |
| GET | `/api/platform-operations/venues` | Lets platform staff browse and filter the list of all venues by name, tier, subscription status and screen health. |
| GET | `/api/platform-operations/venues/{venueId}` | Shows platform staff a support view of one venue: its details, subscription and tier, screens, which features it currently has, and any manual feature overrides. |
| PUT | `/api/platform-operations/venues/{venueId}/overrides/{featureId}` | Lets platform staff manually unlock or block a specific feature for one venue, with a required reason and optional expiry date, regardless of its tier. |
| DELETE | `/api/platform-operations/venues/{venueId}/overrides/{featureId}` | Lets platform staff remove a manual feature override from a venue so it goes back to whatever its tier allows. |
| GET | `/api/platform-operations/onboarding` | Lets platform staff see the 200 most recent customers who signed up and how far each got: organisation created, venue created, tier picked, first screen paired and whether it is online. |

</details>

### Plans, feature switches and tiers

Platform staff set up the paid plans customers can be on — the price, the trial length, and the caps on how many screens and venues a plan allows — and decide which product features each plan includes. Turning a feature on or off for a plan immediately updates what every venue on that plan can do.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Platform staff | `/api/platform-operations/tiers` | 5 | `/api/admin` |
| Platform staff | `/api/platform-operations/features` | 2 | `/api/admin` |

<details><summary>The 7 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/platform-operations/tiers` | Lists every subscription tier (plan) on the platform, including archived ones, for platform staff. |
| POST | `/api/platform-operations/tiers` | Lets platform staff create a new subscription tier with its price, screen and venue limits, trial length, and Stripe price links. |
| PUT | `/api/platform-operations/tiers/{tierId}` | Lets platform staff edit an existing subscription tier's name, price, limits, trial settings and Stripe links. |
| POST | `/api/platform-operations/tiers/{tierId}/clone` | Lets platform staff duplicate a tier as a hidden, inactive draft named '<name> Copy' so they can tweak it before publishing. |
| POST | `/api/platform-operations/tiers/{tierId}/archive` | Lets platform staff retire a tier so no venue can be moved onto it. |
| GET | `/api/platform-operations/features` | Shows platform staff the feature matrix: every tier, every active feature, which tier includes which feature, and the last 50 changes made to it. |
| PUT | `/api/platform-operations/features` | Lets platform staff turn features on or off for tiers in bulk, recording who made the change and refreshing the feature set of every venue on the affected tiers. |

</details>

### Platform settings and moving them between environments

Platform staff can see and change the behind-the-scenes settings the software runs on — one environment at a time — with a full history of who changed what, the ability to put a setting back the way it was, and a safe way to copy a whole set of settings from one environment (say, the test one) into another (say, the live one), previewing every change before it happens. Secret values are never shown, and a change is refused if someone else has edited the same setting in the meantime.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Platform staff | `/api/platform-operations/configuration` | 6 | `/api/admin` |
| Platform staff | `/api/platform-operations/configuration-transfer` | 3 | `/api/admin` |

<details><summary>The 9 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/platform-operations/configuration/health` | Tells platform staff whether the database-backed settings loader is switched on, when it last loaded successfully, and whether its last attempt failed. |
| GET | `/api/platform-operations/configuration?environmentName=&applicationScope=` | Lists the system settings for a chosen environment (and optionally one application), showing each value except for secrets, which are hidden. |
| PUT | `/api/platform-operations/configuration/{definitionId}` | Changes one system setting's value for an environment, refusing if someone else changed it since it was loaded. |
| DELETE | `/api/platform-operations/configuration/{definitionId}` | Removes the stored value for one system setting in an environment so it falls back to its default. |
| GET | `/api/platform-operations/configuration/{definitionId}/revisions?environmentName=` | Shows the change history of one system setting in an environment so staff can see what it used to be and who changed it. |
| POST | `/api/platform-operations/configuration/{definitionId}/rollback` | Puts a system setting back to an earlier version from its history, refusing if the setting changed in the meantime. |
| GET | `/api/platform-operations/configuration-transfer/export?environmentName=` | Downloads a bundle of an environment's non-secret system settings so they can be copied to another environment. |
| POST | `/api/platform-operations/configuration-transfer/preview` | Shows what would change if a settings bundle were applied to a target environment, item by item, without changing anything. |
| POST | `/api/platform-operations/configuration-transfer/apply` | Applies a previously previewed settings bundle to a target environment, refusing if any setting changed since the preview. |

</details>

### Test automation

Gives the automated test suite a few back-door controls over a test venue that no real person gets: wipe the venue clean before a run, lift its menu limit so a big run does not hit the ceiling, and backdate a stock change or a history entry so tests can check how the boards and history screens show older activity.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Test suite | `/api/test-automation` | 4 | — |

<details><summary>The 4 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| POST | `/api/test-automation/availability/backdate` | Lets the automated test suite pretend a menu item was marked in or out of stock some minutes ago, so tests can check how the boards show older stock changes. |
| POST | `/api/test-automation/venues/reset` | Lets the automated test suite wipe a test venue clean (all menus, items, screens, playlists, history and emergency broadcasts) so a test run starts from an empty venue. |
| POST | `/api/test-automation/venues/headroom` | Lets the automated test suite raise a test venue's menu limit to 400 so a full run of seeding tests does not hit the normal 50-menu ceiling. |
| POST | `/api/test-automation/history/write-at` | Lets the automated test suite add a menu history entry stamped with a chosen past date, so tests can check how older history is displayed. |

</details>

### Health and status

Lets anyone confirm the service is up and see exactly which build and database version are running, and gives platform staff a single overview of the whole platform's health — how many venues and paying subscriptions there are, how many screens are online, offline or on an outdated app, and a running list of recent notable changes across all venues.

| Who calls it | Route | Endpoints | Also answers on |
| --- | --- | --- | --- |
| Anyone (no sign-in) | `/ and /health/version` | 2 | — |
| Platform staff | `/api/platform-operations/dashboard` | 2 | `/api/admin` |

<details><summary>The 4 endpoints</summary>

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/` | Answers a simple 'the service is up' check with the service name. |
| GET | `/health/version` | Reports which build of the API is running and which database schema version it is talking to, so deployers can confirm a release landed. |
| GET | `/api/platform-operations/dashboard` | Shows platform staff a health overview: total venues, active/trialing/recently-cancelled subscriptions, how many screens are online, offline or running an outdated app, plus a per-screen list. |
| GET | `/api/platform-operations/dashboard/events` | Shows platform staff the most recent notable events across all venues (tier upgrades/downgrades, feature overrides applied or removed), newest first. |

</details>

---

## Appendix A — things that answer a request but are not in the list above

An inventory built from route markers in controllers misses these. They are real doors
into the same service.

- OIDC sign-in callbacks: the OpenIdConnect authentication middleware itself answers three paths that appear nowhere in Controllers/ or as Map* calls: /signin-customer-google, /signin-customer-apple, and /signin-customer-entra (configured via ConfigureCustomerOidc in /mnt/c/development/vennusign/src/Vennu.Api/Program.cs). These are real HTTP endpoints (POST for Apple's form_post, GET for the others) that complete customer federated login; an attribute-based inventory misses them entirely.

- OpenAPI document: in the Development environment only, app.MapOpenApi() serves the generated OpenAPI JSON (default path /openapi/v1.json). There is no Swagger UI, no AddHealthChecks/MapHealthChecks anywhere in src (the /health/version routes are the hand-rolled MapGets already inventoried), and no static-file or file-server middleware in Vennu.Api.

- AdministrativeCompatibilityMiddleware (/mnt/c/development/vennusign/src/Vennu.Api/Infrastructure/AdministrativeCompatibilityMiddleware.cs) does NOT answer any request itself - it never short-circuits. It inspects each request for six legacy markers: path prefix /api/admin (tag platform-operations-route), path prefix /api/venue-admin (back-office-route), path prefix /hubs/vennu (signalr-route), header X-Vennu-Admin-Key (platform-operations-header), headers X-Vennu-Venue-Token or X-Vennu-Venue-Id (back-office-header), and cookie __Host-Vennu.CustomerSession (customer-session-cookie). On a match it logs 'Legacy administrative contract used' and adds a 'Deprecation: true' response header, then always calls the next middleware. So the legacy routes still work normally - the middleware only marks them deprecated.

- DUAL ROUTES roughly double the URL surface: every controller under Controllers/PlatformOperations has TWO class-level [Route] attributes - api/platform-operations/... plus the legacy api/admin/... - and every controller under Controllers/BackOffice except BackOfficeContentController and BackOfficeMenuImportsController has api/back-office/... plus the legacy api/venue-admin/.... An inventory that lists each [Http*] action once undercounts reachable URLs by ~2x for these ~30 controllers; the legacy variants are the ones the AdministrativeCompatibilityMiddleware flags as deprecated but still serves.

- SignalR: the same VennuHub is mapped at TWO paths - /hubs/vennusign and the legacy /hubs/vennu (both in Program.cs). Each hub route also implicitly exposes the SignalR negotiate endpoint (e.g. POST /hubs/vennu/negotiate). If the inventory listed only one hub route, the legacy one is a miss.

- verified there are no controllers with public actions lacking [Http*] attributes (only constructors are un-attributed public methods), no [AcceptVerbs] anywhere in src, no [NonAction], and no method-level [Route] without a verb attribute - all [Route] attributes are class-level. No controller classes exist outside Controllers/. So item 2 of the checklist is clean.

- Vennu.TestApi (/mnt/c/development/vennusign/src/Vennu.TestApi) IS a separate deployable - its own .csproj, Program.cs, appsettings, and MapControllers. Its endpoints: GET /health/version, plus SeedController at POST /api/test/seed, /api/test/seed/backdate-availability, /api/test/seed/history-at, /api/test/seed/cleanup, and /api/test/seed/scale. Every non-/health request must carry an X-Vennusign-Test-Api-Key header (constant-time SHA-256 comparison in TestApiAuthenticationMiddleware); a missing or wrong key gets a 404, not a 401 - this middleware DOES answer requests itself. It is a test harness, not part of the Venue API process: it calls the product API over HTTP via ProductApiClient using a separate product automation key (the counterpart of Vennu.Api's /api/test-automation endpoints), so it belongs in an audit as an adjacent deployable, not in the Venue API's route table.

- BACKGROUND SERVICES (not endpoints, but run on timers inside the API process): (1) HeartbeatMonitor - every 30s (configurable) marks screens offline whose last heartbeat is older than the 90s stale threshold; (2) ScheduledContentActivationService - every 60s publishes scheduled-content transitions to screens via SignalR; (3) HappyHourEvaluatorService - every 60s publishes happy-hour start/end transitions; (4) PromotionActivationService - every 60s publishes date-range-promotion transitions; (5) PosWebhookWorker - drains the stored POS webhook event queue, signal-driven with a 15s idle poll and a 5-minute processing lease; (6) ToastPollingService (Pos/) - polls Toast per connection for catalog/inventory sync on a configurable interval (default 1 hour, backoff on failure). Note the registration asymmetry: services 2-5 are skipped in the 'Testing' environment, but HeartbeatMonitor and ToastPollingService are registered unconditionally.

- AUTHENTICATION SCHEMES (Program.cs): (1) PlatformOperations (the default scheme) - for internal platform staff; checks the X-Vennusign-Platform-Operations-Key header (legacy: X-Vennu-Admin-Key) against one configured API key via constant-time hash compare, granting the PlatformOperations role plus per-permission configuration claims. (2) BackOffice - for legacy venue-admin clients; checks X-Vennusign-Back-Office-Token (legacy: X-Vennu-Venue-Token) against a config-defined list of per-venue access tokens, and can be globally retired via LegacySessionsEnabled/LegacySessionsRetireAfterUtc. (3) CustomerBackOffice - for signed-in customers managing a venue; reads the customer session cookie (__Host-Vennusign.CustomerSession, legacy __Host-Vennu.CustomerSession), picks the venue from the X-Vennusign-Venue-Id header (legacy X-Vennu-Venue-Id) or the account's onboarded venue, and verifies organization/venue membership. (4) CustomerSession - plain signed-in customer identity from the same session cookie, no venue check. (5) A short-lived (5-minute) external cookie scheme (__Host-Vennusign.CustomerExternal) that only carries OIDC sign-in state mid-flow. (6-8) Three OpenIdConnect schemes - Google, Apple, Microsoft Entra - federated customer login, each owning its /signin-customer-* callback path.

- AUTHORIZATION POLICIES (Program.cs): PlatformOperations policy requires the PlatformOperations role from the platform key scheme; five Configuration:{read|edit|secrets|import|admin} policies each additionally require that permission in the vennusign:configuration_permission claim (permissions come from config next to the platform key); the BackOffice policy accepts EITHER the customer-session-based scheme or the legacy token scheme and requires the BackOffice role plus a venue-id claim; the Customer policy requires an authenticated customer session; the Customer MFA policy adds an MfaSatisfiedRequirement (recent strong authentication) on top of it.

- SMALLER GAPS an endpoint inventory should still note: app.UseHttpsRedirection() answers plain-HTTP requests with redirects; CORS is only enabled when Cors:AllowedOrigins is configured (auto-populated with dev origins in Development) under the single AdministrativePortals policy; and the other directories under src (admin, back-office, display, tv, venue-admin, platform-operations, www, board-engine) are frontend projects, not additional API route sources - Vennu.Api and Vennu.TestApi are the only two projects in src that serve HTTP endpoints.

## Appendix B — how this was produced

Every controller under `src/Vennu.Api/Controllers` was read in full, plus `Program.cs`
and `Hubs/VennuHub.cs`. Endpoint counts per file were taken mechanically from the
`[Http*]` attributes and diffed against what was read — all 44 files matched, with
no re-reads needed. Every endpoint was then assigned to exactly one function; 251 of 251 were assigned, none left over.

**Caveats for anyone building on this.** The plain-English descriptions are a single
reading of each file and have not been independently checked. Behavioural claims were
reasoned from source, not executed — four were spot-checked against the deployed dev API
and one turned out to be wrong (the Clover and Square OAuth callbacks were reported as
blocked by a capability filter; they return 400, not 403, so the filter does not block them).
Treat any individual behavioural claim as a hypothesis until it is run. The route lists,
counts, and route-to-function mapping are reliable.

Some behaviour that looks inconsistent is recorded decision — see
`docs/features/menus/decisions.md` for the Q-numbered menu decisions before treating a
409 code or an odd response shape as a defect.
