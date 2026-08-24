# Camera-based physical commissioning

**Status: Proposed — not yet approved.**

A commissioning aid for Box Players that maps a durable physical Player Output to the real TV an installer is standing in front of. It is separate from the Box Player feature and does not authorize implementation planning.

## The problem it addresses

A multi-output Windows/Linux box may feed TVs through hidden cabling, HDMI-over-Ethernet extenders, or splitters. Windows display numbers can reorder and do not describe the real-world TV location. An installer otherwise has to trace cables or repeatedly guess which output reaches a screen.

The durable output/port identity remains the source of truth. Camera-based commissioning makes the human confirmation of that identity faster and more reliable.

## Proposed operator flow

1. An installer starts a commissioning session for one claimed Box Player.
2. The Player temporarily shows a distinct large marker on every declared output.
3. The installer walks the venue and captures one or more photos or live camera scans. A scan may contain one TV or several TVs; screens do not need to be together.
4. The app recognizes the temporary marker visible on each scanned TV.
5. The installer selects the meaningful Screen/location name, such as “Lobby — Left” or “Upstairs Bar.”
6. The session accumulates a map of **physical location → durable Player Output port key → Screen** and shows a checklist of mapped, unassigned, and disconnected outputs.
7. On confirmation, temporary markers disappear and normal assigned content resumes.

For a six-output box, an installer might scan two lobby TVs in one photo, a bar TV in another, and the remaining screens in separate areas. The session completes only when each declared output is mapped, intentionally left unassigned, or marked disconnected.

## Important boundaries

- The camera does **not** solve Windows display reordering. The durable connector/port key does that.
- The camera does **not** magically know a room or customer-facing name. The installer chooses the Screen/location.
- EDID remains corroborating evidence; it is not the identity.
- The feature must never silently remap content because a panel, cable, Windows display number, or scan result changed.
- Seeing the same marker in two physical locations flags a possible HDMI splitter. The installer must explicitly confirm that relationship.
- This is a commissioning-time aid, not continuous camera monitoring or automatic support capture.

## Intended result

The product retains a truthful record such as:

```text
Lobby — Left    → HDMI-4 / durable PortKey
Main Bar        → HDMI-1 / durable PortKey
Upstairs Hall   → HDMI-6 / durable PortKey
```

Later, if Windows calls the physical HDMI-4 connector “Display 6” instead of “Display 2,” the Lobby content stays on the Lobby TV because its assignment is still bound to HDMI-4’s durable port identity.
