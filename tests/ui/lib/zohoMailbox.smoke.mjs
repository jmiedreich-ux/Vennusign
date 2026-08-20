/**
 * Proves the disposable-mailbox lifecycle end to end against the real Zoho org:
 * sweep -> create -> read -> delete, and confirms the allocation came back.
 *
 * Run: node tests/ui/lib/zohoMailbox.smoke.mjs
 * Requires ~/.config/vennusign-zoho.json (see zohoMailbox.mjs).
 */
import { listMailboxes, messages, withDisposableMailbox, QA_PREFIX } from "./zohoMailbox.mjs";

const before = await listMailboxes();
console.log(`before: ${before.length} mailbox(es) -> ${before.map(m => m.email).join(", ")}`);

const seen = await withDisposableMailbox(async mailbox => {
  console.log(`created: ${mailbox.email} (accountId ${mailbox.accountId})`);
  const during = await listMailboxes();
  console.log(`during: ${during.length} mailbox(es)`);
  const inbox = await messages(mailbox.accountId, 5);
  console.log(`inbox readable: ${inbox.length} message(s)`);
  return mailbox.email;
}, { label: "smoke" });

const after = await listMailboxes();
const leaked = after.filter(m => m.email?.startsWith(QA_PREFIX));
console.log(`after: ${after.length} mailbox(es) -> ${after.map(m => m.email).join(", ")}`);

if (after.length !== before.length || leaked.length > 0) {
  console.error(`FAIL: allocation not returned (leaked: ${leaked.map(m => m.email).join(", ") || "none"})`);
  process.exit(1);
}
console.log(`PASS: ${seen} created, read, and deleted; allocation returned`);
