import { useCallback, useEffect, useRef, useState } from "react";
import { BoardRenderer } from "./BoardRenderer";
import type { BoardRendererProps } from "./BoardRenderer";
import { boardLogicalHeight, boardLogicalWidth, scaleToFit } from "./boardScale.mjs";

export type BoardFrameProps = BoardRendererProps & {
  /**
   * Drawn instead of the board when there is nothing published yet. A menu that
   * has never been published has no board to picture, and inventing one would be
   * a lie about what a screen is showing.
   */
  fallback?: React.ReactNode;
};

/**
 * A board at one fixed logical size, scaled to whatever box it is given.
 *
 * This is the decision that lets a shelf card be honest. The board is always laid
 * out at 1920x1080 and then scaled, so every surface that consumes this engine
 * draws the same DOM at a different scale rather than re-laying out per size.
 *
 * Stated precisely, because the difference matters: the back office consumes it
 * today. The display player still renders the legacy model and adopts this engine
 * in milestone 4, so card-and-TV parity is a property this makes POSSIBLE, not one
 * that holds yet. Nothing here should be read as a claim that the shelf and a real
 * screen already agree - no test exercises both consumers with one board, because
 * there is only one consumer.
 *
 * Content taller than the board is clipped. Pages as a consequence of overflow
 * are milestone 5 and the dwell cycle is milestone 4; a fitting model invented
 * here would be a second layout the player then has to unlearn. Clipping is what
 * an unpaginated TV does, and it is honest about it.
 */
export function BoardFrame({ fallback, ...board }: BoardFrameProps) {
  const container = useRef<HTMLDivElement>(null);
  const [scale, setScale] = useState(1);

  const measure = useCallback(() => {
    const element = container.current;
    if (!element) return;
    setScale(scaleToFit(element.clientWidth, element.clientHeight));
  }, []);

  useEffect(() => {
    measure();

    const element = container.current;
    if (!element || typeof ResizeObserver === "undefined") return;

    const observer = new ResizeObserver(measure);
    observer.observe(element);
    return () => observer.disconnect();
  }, [measure]);

  const hasBoard = (board.board?.sections?.length ?? 0) > 0;

  return (
    <div className="board-frame" ref={container} data-testid="board-frame">
      {hasBoard ? (
        <div
          className="board-frame-stage"
          style={{
            width: `${boardLogicalWidth}px`,
            height: `${boardLogicalHeight}px`,
            transform: `scale(${scale})`,
            ["--board-scale" as string]: scale
          }}
        >
          <BoardRenderer {...board} />
        </div>
      ) : (
        <div className="board-frame-empty" data-testid="board-frame-empty">
          {fallback}
        </div>
      )}
    </div>
  );
}
