import json
import re
import sys
from pathlib import Path

ALLOWED = {
    "Models": set(),
    "Rules": set(),
    "Abstractions": {"Models"},
    "Data": {"Abstractions", "Models", "Rules"},
    "Infrastructure": {"Abstractions", "Data", "Models"},
    "Services": {"Abstractions", "Models"},
    "UseCases": {"Abstractions", "Models", "Rules"},
    "UI": {"Abstractions", "Models", "Rules", "Services", "UseCases"},
    "Program": {"Abstractions", "Data", "Infrastructure", "Services", "UI", "UseCases"},
}

CONCRETE_ONLY = {"StoreRepository": {"Data", "Infrastructure", "Program"}}

root = Path(sys.argv[1]) if len(sys.argv) > 1 else Path(".")
using_re = re.compile(r"^using\s+StoreSystem\.(\w+)", re.M)
found = {name: set() for name in ALLOWED}
errors = []

for path in sorted(root.rglob("*.cs")):
    rel = path.relative_to(root)
    if rel.parts[0] in ("bin", "obj"):
        continue
    package = rel.parts[0] if len(rel.parts) > 1 else "Program"
    if package not in ALLOWED:
        errors.append(f"{rel}: невідомий пакет {package}")
        continue
    text = re.sub(r"//.*", "", path.read_text(encoding="utf-8"))
    for dep in using_re.findall(text):
        if dep == package:
            continue
        found[package].add(dep)
        if dep not in ALLOWED[package]:
            errors.append(f"{rel}: пакет {package} не може залежати від {dep}")
    for cls, owners in CONCRETE_ONLY.items():
        if package not in owners and re.search(rf"\b{cls}\b", text):
            errors.append(f"{rel}: використано конкретний клас {cls}, потрібен інтерфейс")

result = {k: sorted(v) for k, v in found.items()}
if "--json" in sys.argv:
    print(json.dumps(result, ensure_ascii=False, indent=2))
for package, deps in result.items():
    print(f"{package:15} -> {', '.join(deps) if deps else '-'}")
if errors:
    print()
    for e in errors:
        print("ПОМИЛКА:", e)
    sys.exit(1)
print("\nАрхітектурні правила дотримано.")
