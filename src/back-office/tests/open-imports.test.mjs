import { strict as assert } from "node:assert";
import test from "node:test";
import { expiryPhrase, importInProgressPhrase } from "../src/menusShelf.mjs";

/*
 * #904 - an unfinished import was saved for 24 hours and reachable only through browser history.
 * The sentence the shelf now shows is what decides whether somebody goes back in, so it has to be
 * true about time as well as about counts.
 */

const at = (iso) => new Date(iso);

test("names what is left to answer, not what has been done", () => {
  const phrase = importInProgressPhrase(
    { itemCount: 60, answersRemaining: 2, expiresUtc: "2026-08-27T18:47:00Z" },
    at("2026-08-27T06:47:00Z")
  );
  assert.match(phrase, /60 items/);
  assert.match(phrase, /2 answers left/);
});

test("a review with nothing outstanding is waiting, not finished", () => {
  // Zero questions does not mean the import is done - it is sitting at its destination step. An
  // operator told "0 answers left" would reasonably read that as "nothing to come back for".
  const phrase = importInProgressPhrase(
    { itemCount: 12, answersRemaining: 0, expiresUtc: "2026-08-27T18:47:00Z" },
    at("2026-08-27T06:47:00Z")
  );
  assert.match(phrase, /ready to finish/);
  assert.doesNotMatch(phrase, /0 answers/);
});

test("singulars are natural", () => {
  const phrase = importInProgressPhrase(
    { itemCount: 1, answersRemaining: 1, expiresUtc: "2026-08-27T18:47:00Z" },
    at("2026-08-27T06:47:00Z")
  );
  assert.match(phrase, /1 item,/);
  assert.match(phrase, /1 answer left/);
});

test("nothing in, nothing out", () => {
  assert.equal(importInProgressPhrase(null), null);
  assert.equal(importInProgressPhrase(undefined), null);
});

test("today is a time, later this week is a weekday, beyond that is a date", () => {
  // The screen used to print a bare weekday regardless. "Friday" is a lie about anything more than
  // a week out, and a bare time is a lie about anything that is not today.
  const now = at("2026-08-27T06:00:00Z");
  assert.doesNotMatch(expiryPhrase("2026-08-27T18:47:00Z", now), /day/i);
  assert.match(expiryPhrase("2026-08-29T18:47:00Z", now), /Saturday/);
  assert.match(expiryPhrase("2026-09-19T18:47:00Z", now), /Sep/);
});

test("an unreadable expiry is left out rather than printed as Invalid Date", () => {
  assert.equal(expiryPhrase("not a date"), null);
  const phrase = importInProgressPhrase({ itemCount: 4, answersRemaining: 1, expiresUtc: "not a date" });
  assert.match(phrase, /4 items, 1 answer left/);
  assert.doesNotMatch(phrase, /saved until/);
  assert.doesNotMatch(phrase, /Invalid/);
});
