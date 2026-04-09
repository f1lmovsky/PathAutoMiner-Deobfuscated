"""Validate minified C# for common issues that would break SE."""
import sys, re

path = sys.argv[1] if len(sys.argv) > 1 else "PAM_Minified.cs"
with open(path, "r", encoding="utf-8") as f:
    code = f.read()

errors = []

# 1. Check balanced delimiters
for open_c, close_c, name in [('(',')', 'parens'), ('{','}', 'braces'), ('[',']', 'brackets')]:
    depth = 0
    in_str = False
    in_char = False
    i = 0
    while i < len(code):
        c = code[i]
        if not in_str and not in_char:
            if c == '"': in_str = True
            elif c == "'": in_char = True
            elif c == open_c: depth += 1
            elif c == close_c: depth -= 1
            if depth < 0:
                line = code[:i].count('\n') + 1
                col = i - code.rfind('\n', 0, i)
                errors.append(f"UNBALANCED {name}: extra '{close_c}' at line {line}, col {col}")
                depth = 0
        elif in_str:
            if c == '\\': i += 1
            elif c == '"': in_str = False
        elif in_char:
            if c == '\\': i += 1
            elif c == "'": in_char = False
        i += 1
    if depth != 0:
        errors.append(f"UNBALANCED {name}: {depth} unclosed '{open_c}'")

# 2. Check critical identifiers exist
required = [
    'void Main(', 'Program()', 'void Save()',
    'UpdateFrequency.Update10', 'GridTerminalSystem',
    'WriteText', 'Echo(', 'case "UP"', 'case "STOP"',
    'case "START"', 'case "PATHHOME"', 'ContainsKey', 'TryGetValue',
    'IMyRemoteControl', 'IMyThrust', 'IMyGyro',
    'GetBlocksOfType', '.Keys', '.Values',
    'ElementAt(', 'ElementAtOrDefault(',
]
for ident in required:
    if ident not in code:
        errors.append(f"MISSING required identifier: {ident}")

# 3. Check for keyword-string spacing issues
for kw in ['case', 'return', 'throw', 'new', 'typeof']:
    pattern = kw + '"'
    if pattern in code:
        idx = code.index(pattern)
        line = code[:idx].count('\n') + 1
        errors.append(f"MISSING SPACE: '{pattern}' at line {line}")

# 4. Check for suspicious patterns
# Double semicolons (usually harmless but suspicious)
doubles = len(re.findall(r';;', code))
if doubles > 5:
    errors.append(f"WARNING: {doubles} double semicolons (;;)")

# 5. Check line lengths
for i, line in enumerate(code.split('\n'), 1):
    if len(line) > 100000:
        errors.append(f"LINE TOO LONG: line {i} has {len(line)} chars")

# 6. Check Unicode identifier validity
# C# allows: Lu, Ll, Lt, Lm, Lo, Nl categories for identifiers
import unicodedata
bad_chars = set()
for c in code:
    if ord(c) > 127:
        cat = unicodedata.category(c)
        if cat not in ('Lu', 'Ll', 'Lt', 'Lm', 'Lo', 'Nl', 'Mn', 'Mc', 'Nd', 'Pc', 'Cf'):
            bad_chars.add((c, hex(ord(c)), cat))
if bad_chars:
    for c, h, cat in sorted(bad_chars, key=lambda x: x[1]):
        errors.append(f"SUSPICIOUS UNICODE: '{c}' ({h}) category={cat}")

# 7. Verify enum values are intact
enums_to_check = [
    ('ShipMode', ['Unknown', 'Miner', 'Grinder', 'Controller', 'Shuttle']),
    ('NavState', ['Idle', 'FlyToXY', 'Mining']),
    ('JobState', ['NoJob', 'Paused', 'Active']),
]
# These enum VALUE NAMES are user-defined and WILL be renamed, so skip

# 8. Check for split-specific issues: verify no section comment markers leaked
if '// ==========================' in code:
    errors.append("SECTION COMMENT leaked into minified output")

print(f"Validating: {path}")
print(f"Size: {len(code)} chars")
print()

if errors:
    print(f"FOUND {len(errors)} issues:")
    for e in errors:
        print(f"  - {e}")
else:
    print("ALL CHECKS PASSED")
