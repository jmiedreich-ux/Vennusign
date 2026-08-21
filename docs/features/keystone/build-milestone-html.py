import html
import re

D = "/mnt/c/development/vennusign/.claude/worktrees/keystone-decisions/docs/features/keystone/"
OUT = "/home/jeremy/.claude/jobs/17e917e8/tmp/keystone-milestones.html"

DOCS = [("index", "Index", D + "milestone-plan.md")] + [
    ("m%d" % n, "M%d" % n, D + "m%d-plan.md" % n) for n in range(1, 7)
]


def inline(text):
    spans = []

    def stash(m):
        spans.append(html.escape(m.group(1)))
        return "\x00%d\x00" % (len(spans) - 1)

    text = re.sub(r"`([^`]+)`", stash, text)
    text = html.escape(text)
    text = re.sub(r"\*\*(.+?)\*\*", r"<strong>\1</strong>", text)
    text = re.sub(r"(?<![\w*])\*([^*]+)\*(?![\w*])", r"<em>\1</em>", text)
    text = re.sub(r"\x00(\d+)\x00", lambda m: "<code>%s</code>" % spans[int(m.group(1))], text)
    return text


def step_kind(title):
    t = title.lower()
    if "commit" in t:
        return "commit", "commit"
    if "confirm it fails" in t or "confirm they fail" in t or "verify it fails" in t or "verify they fail" in t:
        return "expect-fail", "expect fail"
    if ("confirm it passes" in t or "confirm they pass" in t or "verify it passes" in t
            or "verify they pass" in t or "verification" in t or "run the full" in t):
        return "expect-pass", "expect pass"
    if "failing test" in t:
        return "test", "write test"
    return "build", "implement"


def convert(md, first_heading_is_title=True):
    lines = md.split("\n")
    out, i, in_task, title = [], 0, False, ""

    while i < len(lines):
        line = lines[i]

        if line.startswith("```"):
            lang = line[3:].strip()
            i += 1
            buf = []
            while i < len(lines) and not lines[i].startswith("```"):
                buf.append(lines[i])
                i += 1
            i += 1
            label = ('<span class="lang">%s</span>' % html.escape(lang)) if lang else ""
            out.append('<div class="code">%s<pre>%s</pre></div>' % (label, html.escape("\n".join(buf))))
            continue

        if line.startswith("|") and i + 1 < len(lines) and set(lines[i + 1].replace("|", "").strip()) <= set("-: "):
            header = [c.strip() for c in line.strip().strip("|").split("|")]
            i += 2
            rows = []
            while i < len(lines) and lines[i].startswith("|"):
                rows.append([c.strip() for c in lines[i].strip().strip("|").split("|")])
                i += 1
            thead = "".join("<th>%s</th>" % inline(c) for c in header)
            tbody = "".join("<tr>%s</tr>" % "".join("<td>%s</td>" % inline(c) for c in r) for r in rows)
            out.append('<div class="tablewrap"><table><thead><tr>%s</tr></thead><tbody>%s</tbody></table></div>'
                       % (thead, tbody))
            continue

        if line.startswith("# "):
            title = line[2:].strip()
            i += 1
            continue

        if line.startswith("### Task "):
            if in_task:
                out.append("</div></section>")
            out.append('<section class="task"><h4 class="task-head" tabindex="0" role="button" '
                       'aria-expanded="false">%s</h4><div class="task-body">' % inline(line[4:].strip()))
            in_task = True
            i += 1
            continue

        if line.startswith("## "):
            if in_task:
                out.append("</div></section>")
                in_task = False
            out.append("<h3>%s</h3>" % inline(line[3:].strip()))
            i += 1
            continue

        m = re.match(r"- \[ \] \*\*(.+?)\*\*\s*$", line)
        if m:
            kind, label = step_kind(m.group(1))
            out.append('<div class="step %s"><label><input type="checkbox"><span class="stitle">%s</span></label>'
                       '<span class="kind">%s</span></div>' % (kind, inline(m.group(1)), label))
            i += 1
            continue

        if line.startswith("> "):
            buf = []
            while i < len(lines) and lines[i].startswith("> "):
                buf.append(lines[i][2:])
                i += 1
            out.append('<blockquote>%s</blockquote>' % inline(" ".join(buf)))
            continue

        if line.strip() == "---":
            out.append("<hr>")
            i += 1
            continue

        if line.startswith("- "):
            buf = []
            while i < len(lines) and lines[i].startswith("- "):
                item = lines[i][2:]
                i += 1
                while i < len(lines) and lines[i].startswith("  ") and lines[i].strip():
                    item += " " + lines[i].strip()
                    i += 1
                buf.append(item)
            out.append("<ul>%s</ul>" % "".join("<li>%s</li>" % inline(b) for b in buf))
            continue

        if line.strip() == "":
            i += 1
            continue

        buf = [line]
        i += 1
        while i < len(lines) and lines[i].strip() and not re.match(r"^(#|-|>|\||```)", lines[i]):
            buf.append(lines[i])
            i += 1
        out.append("<p>%s</p>" % inline(" ".join(buf)))

    if in_task:
        out.append("</div></section>")
    return title, "\n".join(out)


sections, nav, total_tasks = [], [], 0
for anchor, label, path in DOCS:
    md = open(path, encoding="utf-8").read()
    title, body = convert(md)
    ntasks = body.count('class="task-head"')
    total_tasks += ntasks
    meta = ('<span class="mcount">%d tasks</span>' % ntasks) if ntasks else ""
    sections.append('<section class="doc" id="%s"><h2>%s%s</h2>%s</section>'
                    % (anchor, inline(title), meta, body))
    nav.append('<a href="#%s">%s</a>' % (anchor, html.escape(label)))

TEMPLATE = """<title>Keystone Milestone Plan</title>
<link rel="preconnect" href="https://fonts.googleapis.com">
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=IBM+Plex+Mono:wght@400;500&family=IBM+Plex+Sans:wght@400;500;600&family=IBM+Plex+Serif:wght@400;500;600&display=swap">
<style>
:root{--ink:#1b1f24;--muted:#5b6470;--faint:#8c94a1;--line:#c7cdd6;--line-soft:#e2e6ec;--surface:#ffffff;--page:#f3f5f8;--accent:#b45309;--accent-ink:#8a3f07;--accent-soft:#fdf0e1;--on-accent:#ffffff;--fill:#eef2f8;--done:#2d6a4f;--code-bg:#f7f9fb;--shadow:0 1px 2px rgba(27,31,36,.06),0 4px 12px rgba(27,31,36,.05);--sans:"IBM Plex Sans",ui-sans-serif,system-ui,-apple-system,"Segoe UI",sans-serif;--serif:"IBM Plex Serif",Georgia,serif;--mono:"IBM Plex Mono",ui-monospace,"SF Mono",Menlo,monospace}
@media (prefers-color-scheme:dark){:root:not([data-theme="light"]){--ink:#e8eaed;--muted:#a3acb8;--faint:#79828e;--line:#333a44;--line-soft:#262d35;--surface:#1c2127;--page:#12161a;--accent:#e08c3c;--accent-ink:#f0a55c;--accent-soft:#2e2317;--on-accent:#241a10;--fill:#232a33;--done:#74c69d;--code-bg:#171c22;--shadow:0 1px 2px rgba(0,0,0,.4),0 4px 12px rgba(0,0,0,.3)}}
:root[data-theme="dark"]{--ink:#e8eaed;--muted:#a3acb8;--faint:#79828e;--line:#333a44;--line-soft:#262d35;--surface:#1c2127;--page:#12161a;--accent:#e08c3c;--accent-ink:#f0a55c;--accent-soft:#2e2317;--on-accent:#241a10;--fill:#232a33;--done:#74c69d;--code-bg:#171c22;--shadow:0 1px 2px rgba(0,0,0,.4),0 4px 12px rgba(0,0,0,.3)}
*{box-sizing:border-box}
body{margin:0;background:var(--page);color:var(--ink);font-family:var(--sans);font-size:16px;line-height:1.55;-webkit-text-size-adjust:100%}
code{font-family:var(--mono);font-size:.87em;background:var(--fill);padding:1px 5px;border-radius:4px;overflow-wrap:break-word}
a{color:var(--accent-ink)}
a:focus-visible,input:focus-visible,.task-head:focus-visible{outline:2px solid var(--accent);outline-offset:2px}
header{position:sticky;top:0;z-index:20;background:var(--surface);border-bottom:1px solid var(--line);padding:.75rem 1rem .6rem}
.masthead{max-width:48rem;margin:0 auto;display:flex;align-items:baseline;gap:.75rem}
.masthead h1{margin:0;font-size:1rem;font-weight:600;letter-spacing:-.01em;flex:1 1 auto}
.masthead .of{font-family:var(--mono);font-size:.78rem;color:var(--faint);white-space:nowrap}
nav{max-width:48rem;margin:.55rem auto 0;display:flex;gap:.35rem;overflow-x:auto;scrollbar-width:none}
nav::-webkit-scrollbar{display:none}
nav a{flex:0 0 auto;font-size:.8rem;font-weight:500;padding:.32rem .7rem;border-radius:999px;border:1px solid var(--line);color:var(--muted);text-decoration:none;white-space:nowrap}
nav a:hover{color:var(--accent-ink);border-color:var(--faint)}
main{max-width:48rem;margin:0 auto;padding:1.25rem 1rem 5rem}
.doc{margin-bottom:2.5rem}
.doc>h2{display:flex;align-items:baseline;gap:.6rem;font-family:var(--serif);font-size:1.2rem;font-weight:600;margin:0 0 1rem;padding-bottom:.5rem;border-bottom:2px solid var(--accent);scroll-margin-top:5.5rem;text-wrap:balance}
.mcount{margin-left:auto;font-family:var(--mono);font-size:.72rem;font-weight:400;color:var(--faint);white-space:nowrap}
h3{font-size:.74rem;font-weight:600;letter-spacing:.1em;text-transform:uppercase;color:var(--faint);margin:1.6rem 0 .7rem;padding-bottom:.35rem;border-bottom:1px solid var(--line)}
p{margin:.6rem 0;text-wrap:pretty}
ul{margin:.55rem 0;padding-left:1.15rem}
li{margin:.3rem 0;text-wrap:pretty}
hr{border:none;border-top:1px solid var(--line-soft);margin:1.5rem 0}
blockquote{margin:.9rem 0;padding:.75rem .85rem;background:var(--accent-soft);border-left:3px solid var(--accent);border-radius:0 8px 8px 0;font-size:.9rem}
.task{background:var(--surface);border:1px solid var(--line);border-radius:10px;box-shadow:var(--shadow);margin:.75rem 0;overflow:hidden}
.task-head{margin:0;padding:.8rem .9rem;font-family:var(--serif);font-size:1rem;font-weight:600;cursor:pointer;user-select:none;display:flex;align-items:center;gap:.5rem}
.task-head::before{content:"\\25B8";color:var(--faint);font-size:.8em;flex:0 0 auto}
.task-head[aria-expanded="true"]::before{content:"\\25BE"}
.task-head[aria-expanded="false"]+.task-body{display:none}
.task-body{padding:0 .9rem 1rem;border-top:1px solid var(--line-soft)}
.step{display:flex;align-items:center;gap:.6rem;margin:.8rem 0 .3rem;padding:.5rem .65rem;border-radius:8px;background:var(--fill)}
.step label{display:flex;align-items:center;gap:.5rem;flex:1 1 auto;cursor:pointer;min-width:0}
.step input{flex:0 0 auto;width:16px;height:16px;accent-color:var(--done)}
.step .stitle{font-weight:600;font-size:.92rem}
.step input:checked+.stitle{color:var(--faint);text-decoration:line-through}
.kind{flex:0 0 auto;font-family:var(--mono);font-size:.66rem;letter-spacing:.04em;padding:.14rem .38rem;border-radius:4px;white-space:nowrap;border:1px solid var(--line);color:var(--muted)}
.step.test .kind{border-color:var(--accent);color:var(--accent-ink)}
.step.expect-fail .kind{border-color:var(--accent);background:var(--accent-soft);color:var(--accent-ink)}
.step.expect-pass .kind{border-color:var(--done);color:var(--done)}
.step.commit .kind{background:var(--ink);border-color:var(--ink);color:var(--surface)}
.code{position:relative;margin:.55rem 0}
.code .lang{position:absolute;top:.4rem;right:.55rem;font-family:var(--mono);font-size:.66rem;color:var(--faint);letter-spacing:.05em}
pre{margin:0;padding:.75rem .85rem;background:var(--code-bg);border:1px solid var(--line-soft);border-radius:8px;overflow-x:auto;font-family:var(--mono);font-size:.76rem;line-height:1.65;color:var(--ink)}
.tablewrap{overflow-x:auto;margin:.75rem 0}
table{border-collapse:collapse;width:100%;font-size:.88rem}
th{text-align:left;font-size:.68rem;font-weight:600;letter-spacing:.07em;text-transform:uppercase;color:var(--faint);padding:.45rem .6rem;border-bottom:1px solid var(--line)}
td{padding:.5rem .6rem;border-bottom:1px solid var(--line-soft);vertical-align:top;color:var(--muted)}
@media (min-width:680px){body{font-size:16.5px}pre{font-size:.79rem}}
@media (prefers-reduced-motion:reduce){*{transition:none!important}}
</style>
<header>
  <div class="masthead"><h1>Keystone Milestone Plan</h1><span class="of">6 · __TASKS__ tasks</span></div>
  <nav>__NAV__</nav>
</header>
<main>__BODY__</main>
<script>
document.querySelectorAll(".task-head").forEach(function (h) {
  function toggle() { h.setAttribute("aria-expanded", h.getAttribute("aria-expanded") === "true" ? "false" : "true"); }
  h.addEventListener("click", toggle);
  h.addEventListener("keydown", function (e) { if (e.key === "Enter" || e.key === " ") { e.preventDefault(); toggle(); } });
});
var KEY = "keystone.milestones.v1", state = {};
try { state = JSON.parse(localStorage.getItem(KEY) || "{}") || {}; } catch (e) { state = {}; }
document.querySelectorAll(".step input").forEach(function (box, n) {
  if (state["s" + n]) { box.checked = true; }
  box.addEventListener("change", function () {
    state["s" + n] = box.checked;
    try { localStorage.setItem(KEY, JSON.stringify(state)); } catch (e) {}
  });
});
</script>
"""

open(OUT, "w", encoding="utf-8").write(
    TEMPLATE.replace("__NAV__", "".join(nav))
            .replace("__BODY__", "\n".join(sections))
            .replace("__TASKS__", str(total_tasks)))
print("wrote", OUT, "docs:", len(DOCS), "tasks:", total_tasks)
