/** The one logical size every board is laid out at, before it is scaled to a box. */
export const boardLogicalWidth: number;

export const boardLogicalHeight: number;

export const boardAspectRatio: number;

/**
 * The scale that fits the logical board into the given box without distorting it.
 * Zero when the box has no measurable size yet, so nothing flashes at full size.
 */
export function scaleToFit(boxWidth: number, boxHeight: number): number;
