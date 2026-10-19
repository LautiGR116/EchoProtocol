from pathlib import Path
import urllib.request,json,hashlib,datetime
root=Path('/Users/lautigr116/EchoProtocol');dest=root/'Assets/_Game/Art/Folio/Fonts';source=root/'ArtSource/Design/Folio/Fonts';dest.mkdir(parents=True,exist_ok=True);source.mkdir(exist_ok=True)
requests=[('IBM/plex','packages/plex-sans/fonts/complete/ttf/IBMPlexSans-Regular.ttf'),('IBM/plex','packages/plex-sans/fonts/complete/ttf/IBMPlexSans-Medium.ttf'),('IBM/plex','packages/plex-serif/fonts/complete/ttf/IBMPlexSerif-Regular.ttf'),('IBM/plex','packages/plex-serif/fonts/complete/ttf/IBMPlexSerif-Italic.ttf'),('CatharsisFonts/Cormorant','fonts/ttf/CormorantGaramond-SemiBold.ttf'),('CatharsisFonts/Cormorant','fonts/ttf/CormorantGaramond-Italic.ttf'),('jpt/barlow','fonts/ttf/BarlowCondensed-Medium.ttf')]
rows=[]
for repo,path in requests:
 tree=json.load(open('/tmp/'+repo.split('/')[1]+'-font-tree.json'));assert any(x['path']==path for x in tree['tree']);url=f'https://raw.githubusercontent.com/{repo}/{tree["sha"]}/{path}';data=urllib.request.urlopen(url).read();p=dest/Path(path).name;assert not p.exists();p.write_bytes(data);rows.append(dict(file=str(p.relative_to(root)),source=url,repository='https://github.com/'+repo,revision=tree['sha'],bytes=len(data),sha256=hashlib.sha256(data).hexdigest(),license='SIL OFL 1.1',modified=False))
for repo,path in [('IBM/plex','LICENSE.txt'),('CatharsisFonts/Cormorant','OFL.txt'),('jpt/barlow','OFL.txt')]:
 tree=json.load(open('/tmp/'+repo.split('/')[1]+'-font-tree.json'));url=f'https://raw.githubusercontent.com/{repo}/{tree["sha"]}/{path}';data=urllib.request.urlopen(url).read();assert b'OPEN FONT LICENSE' in data.upper();(source/(repo.split('/')[1]+'-OFL.txt')).write_bytes(data)
(source/'manifest.json').write_text(json.dumps(dict(dateUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),files=rows),indent=2)+'\n');print([(Path(r['file']).name,r['bytes']) for r in rows])
