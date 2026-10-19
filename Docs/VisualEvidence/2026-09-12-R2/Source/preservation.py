from pathlib import Path
import re,hashlib
before=Path('/tmp/echo-vs1-r2/before.unity').read_text()
after=Path('Assets/_Game/Scenes/Development/VS1_ParallelReturn_Blockout.unity').read_text()
def records(s):
 return {m.group(1):(m.group(2),m.group(0)) for m in re.finditer(r'^--- !u!\d+ &(\d+)\n(.*?)(?=^--- !u!|\Z)',s,re.M|re.S)}
a,b=records(before),records(after)
changes=[k for k in a if k in b and a[k]!=b[k]]
removed=set(a)-set(b)
lines=[f'Original records: {len(a)}; retained: {len(set(a)&set(b))}; removed: {len(removed)}; changed: {len(changes)}; new: {len(set(b)-set(a))}']
for k in changes:
 name=re.search(r'm_Name: (.*)',a[k][0])
 lines.append(f'Changed {k}: '+(name.group(1) if name else a[k][0].splitlines()[0]))
lines.append('LoopController byte-identical to task start: '+str(Path('/tmp/echo-vs1-r2/LoopController.before.cs').read_bytes()==Path('Assets/_Game/Scripts/Echo/LoopController.cs').read_bytes()))
# Preserved MonoBehaviours except the active loop's appended reset list.
mono_changed=[k for k in changes if a[k][0].startswith('MonoBehaviour:')]
lines.append('Changed existing MonoBehaviours: '+str(mono_changed))
Path('/tmp/echo-vs1-r2/preservation.txt').write_text('\n'.join(lines)+'\n')
print('\n'.join(lines))
