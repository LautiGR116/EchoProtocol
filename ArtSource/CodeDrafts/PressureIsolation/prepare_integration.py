"""Prepare reviewable drafts only. This script never writes inside Assets."""
from pathlib import Path
import difflib
import hashlib
import json

repo = Path(__file__).resolve().parents[3]
out = Path(__file__).resolve().parent
patches = []
sources = {}

def prepare(relative, edit):
    path = repo / relative
    original = path.read_text()
    changed = edit(original)
    (out / (path.stem + '.cs.txt')).write_text(changed)
    sources[relative] = hashlib.sha256(original.encode()).hexdigest()
    patches.extend(difflib.unified_diff(original.splitlines(True), changed.splitlines(True),
                                      fromfile='a/' + relative, tofile='b/' + relative))

def replace(text, old, new):
    if text.count(old) != 1:
        raise RuntimeError('Expected one integration anchor: ' + old[:100])
    return text.replace(old, new)

def director(s):
    s = replace(s, '        [SerializeField] private HouseMemory house;',
                '        [SerializeField] private HouseMemory house;\n        [SerializeField] private PressureIsolation pressureIsolation;')
    s = replace(s, 'public bool IsReturnRecordingSuspended=>returnRecordingBoundary&&returnRecordingBoundary.IsSuspended;',
                'public bool UsesPressureIsolation=>pressureIsolation;\n        public PressureIsolation IsolationControl=>pressureIsolation;\n        public bool IsReturnRecordingSuspended=>returnRecordingBoundary&&returnRecordingBoundary.IsSuspended&&(!pressureIsolation||Chapter<CampaignChapter.Pressure);')
    s = replace(s, 'public string StatusText=>IsReturnRecordingSuspended?returnRecordingBoundary.StatusText:status;',
                'public string StatusText=>IsReturnRecordingSuspended?returnRecordingBoundary.StatusText:pressureIsolation&&Chapter==CampaignChapter.Pressure?pressureIsolation.StatusText:status;')
    s = replace(s, 'public float Pressure=>pressure;', 'public float Pressure=>pressureIsolation?pressureIsolation.Pressure01:pressure;')
    s = replace(s, 'public string Objective {get=>IsReturnRecordingSuspended?returnRecordingBoundary.Objective:objective;private set=>objective=value;}',
                'public string Objective {get=>IsReturnRecordingSuspended?returnRecordingBoundary.Objective:pressureIsolation&&Chapter==CampaignChapter.Pressure?pressureIsolation.Objective:objective;private set=>objective=value;}')
    s = replace(s, 'public bool HasSavedCheckpoint=>PlayerPrefs.GetInt(SaveKey,0)>=2&&PlayerPrefs.GetInt(SaveKey,0)<=5;',
                'public bool HasSavedCheckpoint {get {int saved=PlayerPrefs.GetInt(SaveKey,0);return saved>=2&&saved<=5&&(!pressureIsolation||saved!=4);}}')
    s = replace(s, 'public string SavedChapterTitle=>finalDoor&&PlayerPrefs.GetInt(SaveKey,0)==5?',
                'public string SavedChapterTitle=>pressureIsolation&&PlayerPrefs.GetInt(SaveKey,0)==2?"Subsuelo / aislamiento":finalDoor&&PlayerPrefs.GetInt(SaveKey,0)==5?')
    old = 'private void SaveCheckpoint(){if(!SuppressSaves){PlayerPrefs.SetInt(SaveKey,(int)Chapter);if(finalDoor&&Chapter==CampaignChapter.Archive){PlayerPrefs.SetInt(SaveKey+".answered",house.HasAnswered?1:0);PlayerPrefs.SetInt(SaveKey+".light",(int)house.PowerLocation);}PlayerPrefs.Save();}}'
    new = '''private void SaveCheckpoint()
        {
            if(SuppressSaves)return;
            PlayerPrefs.SetInt(SaveKey,(int)Chapter);
            if(pressureIsolation)PlayerPrefs.SetInt(SaveKey+".isolated",pressureIsolation.IsLatched?1:0);
            if(finalDoor&&Chapter==CampaignChapter.Archive)
            {
                PlayerPrefs.SetInt(SaveKey+".answered",house.HasAnswered?1:0);
                PlayerPrefs.SetInt(SaveKey+".light",(int)house.PowerLocation);
            }
            PlayerPrefs.Save();
        }'''
    s = replace(s, old, new)
    s = replace(s, 'PlayerPrefs.DeleteKey(SaveKey+".light");PlayerPrefs.Save();}',
                'PlayerPrefs.DeleteKey(SaveKey+".light");PlayerPrefs.DeleteKey(SaveKey+".isolated");PlayerPrefs.Save();}')
    s = replace(s, '            if(saved>=CampaignChapter.House){valveA=valveB=true;doors[1].SetOpen(true);machinery.Stop();}',
                '''            if(pressureIsolation)
            {
                bool isolated=saved>=CampaignChapter.House||PlayerPrefs.GetInt(SaveKey+".isolated",0)==1;
                if(!pressureIsolation.RestoreState(isolated))return false;
                if(saved==CampaignChapter.Pressure)
                {
                    var safeOrigin=pressureIsolation.ContinuationOrigin;
                    player.ResetPose(safeOrigin.position,safeOrigin.rotation);
                    Change(CampaignChapter.Pressure,pressureIsolation.Objective);
                    return true;
                }
            }
            if(saved>=CampaignChapter.House){valveA=valveB=true;doors[1].SetOpen(true);if(!pressureIsolation)machinery.Stop();}''')
    s = replace(s, 'player.ResetPose(origins[index].position,origins[index].rotation);Origin(index);',
                'player.ResetPose(origins[index].position,origins[index].rotation);if(!pressureIsolation)Origin(index);')
    s = replace(s, 'Change(CampaignChapter.Pressure,"ISOLATED HOUSE TEST");}',
                'Change(CampaignChapter.Pressure,"ISOLATED HOUSE TEST");if(pressureIsolation)pressureIsolation.RestoreState(true);}')
    s = replace(s, 'case StationKind.PressureOrigin:return Chapter==CampaignChapter.Descent;',
                'case StationKind.PressureOrigin:return !pressureIsolation&&Chapter==CampaignChapter.Descent;')
    s = replace(s, 'case StationKind.ValveA:return Chapter==CampaignChapter.Pressure&&!valveA;',
                'case StationKind.ValveA:return !pressureIsolation&&Chapter==CampaignChapter.Pressure&&!valveA;')
    s = replace(s, 'case StationKind.ValveB:return Chapter==CampaignChapter.Pressure&&!valveB;',
                'case StationKind.ValveB:return !pressureIsolation&&Chapter==CampaignChapter.Pressure&&!valveB;')
    s = replace(s, 'case StationKind.HouseOrigin:return Chapter==CampaignChapter.Pressure&&valveA&&valveB;',
                'case StationKind.HouseOrigin:return Chapter==CampaignChapter.Pressure&&(pressureIsolation?pressureIsolation.IsDrained:valveA&&valveB);')
    s = replace(s, 'Origin(1);loop.enabled=false;recorder.CancelRecording();machinery.Stop();',
                'if(!pressureIsolation)Origin(1);loop.enabled=false;recorder.CancelRecording();if(!pressureIsolation)machinery.Stop();')
    s = replace(s, '            if(Chapter==CampaignChapter.Pressure)UpdatePressure();',
                '''            if(Chapter==CampaignChapter.Pressure)
            {
                if(pressureIsolation)return; // Its physical control owns pressure, lamps and sound.
                UpdatePressure();
            }''')
    s = replace(s, '        private void Change(CampaignChapter chapter,string objective)',
                '''        // Only the opt-in physical entry volume calls this. No new recording origin.
        public bool BeginPressureIsolation()
        {
            if(!pressureIsolation||Chapter!=CampaignChapter.Descent||session.IsPaused||!player.isActiveAndEnabled)return false;
            if(!pressureIsolation.EnterService())return false;
            Change(CampaignChapter.Pressure,pressureIsolation.Objective);
            SaveCheckpoint();
            return true;
        }
        public void CommitPressureIsolation()
        {
            if(!pressureIsolation||!pressureIsolation.IsLatched||Chapter!=CampaignChapter.Pressure||session.IsPaused)return;
            SaveCheckpoint();
        }
        private void Change(CampaignChapter chapter,string objective)''')
    return s

def door(s):
    return replace(s, '        public void ResetLoopState()', '''        // Explicit chapter restoration; normal play still uses animated SetOpen.
        public void SetOpenImmediately(bool shouldOpen)
        {
            IsOpenRequested = shouldOpen;
            if (doorBody == null) return;
            Vector3 local = shouldOpen ? openLocalPosition : closedLocalPosition;
            doorBody.position = GetWorldPosition(local);
            doorBody.transform.localPosition = local;
            Physics.SyncTransforms();
        }

        public void ResetLoopState()''')

def session(s):
    old_prompt = '            if(target && !(campaign && campaign.IsWorking)) GUI.Label(new Rect(535,406,590,32),"[E]  "+(t is CampaignStation cs?cs.Prompt:t is HouseInteraction hi?hi.Prompt:t is FinalDoorInteraction fi?fi.Prompt:"Interactuar"),accent);'
    new_prompt = '''            var isolation=campaign?campaign.IsolationControl:null;
            if(isolation&&isolation.IsHolding)
                GUI.Label(new Rect(535,406,590,32),"Mantener E · aislar la instalación",accent);
            else if(target && !(campaign && campaign.IsWorking))
                GUI.Label(new Rect(535,406,590,32),t is PressureIsolation?"Mantener E · aislar la instalación":"[E]  "+(t is CampaignStation cs?cs.Prompt:t is HouseInteraction hi?hi.Prompt:t is FinalDoorInteraction fi?fi.Prompt:"Interactuar"),accent);'''
    s = replace(s, old_prompt, new_prompt)
    old = r'''                GUI.Label(new Rect(310,237,660,250),"RECALL / NOTAS DE LA SESIÓN\n\n"+context+"\n\nLas estaciones [E] fijan el origen del próximo RECALL.\nH cierra estas notas. La sesión sigue en marcha.",text);'''
    new = r'''                string recallNote="Las estaciones [E] fijan el origen del próximo RECALL.";
                if(campaign&&campaign.UsesPressureIsolation)
                    recallNote=finalDoorMemory&&finalDoorMemory.Active
                        ? (finalDoorMemory.OriginalPreserved?"El original está conservado. El registro terminó.":"R vuelve a 6B y sustituye la toma que estás revisando.")
                        : !loop.enabled?"El registro permanece suspendido hasta la próxima revisión."
                        : "Sólo una estación marcada ORIGEN fija el próximo RECALL.";
                GUI.Label(new Rect(310,237,660,250),"RECALL / NOTAS DE LA SESIÓN\n\n"+context+"\n\n"+recallNote+"\nH cierra estas notas. La sesión sigue en marcha.",text);'''
    return replace(s, old, new)

def boundary(s):
    s = replace(s, '        private bool initialized, suspended;',
                '        [SerializeField] private string followupObjective = "Volvé al vestíbulo. El próximo origen está en el subsuelo.";\n        private bool initialized, suspended;')
    return replace(s, 'public string Objective => "Volvé al vestíbulo. El próximo origen está en el subsuelo.";',
                   'public string Objective => followupObjective;')

def tone(s):
    s = replace(s, '        [SerializeField] VS1StrongEventOne firstActEvent;',
                '''        [SerializeField] VS1StrongEventOne firstActEvent;
        [SerializeField] PressureIsolation serviceIsolation;
        [Tooltip("Only institutional motor beds supplied by this plant; exclude room air and the local motor owned by PressureIsolationAudio.")]
        [SerializeField] AudioSource[] isolatedMotorBeds = new AudioSource[0];''')
    s = replace(s, '                institution[i].volume=Mathf.MoveTowards(institution[i].volume,target,Time.deltaTime*rate);',
                '''                if(serviceIsolation && IsIsolatedMotor(institution[i]))
                {
                    target*=serviceIsolation.MotorLevel01;
                    if(serviceIsolation.IsDrained){institution[i].volume=0f;continue;}
                }
                institution[i].volume=Mathf.MoveTowards(institution[i].volume,target,Time.deltaTime*rate);''')
    s = replace(s, '        void Update()',
                '''        bool IsIsolatedMotor(AudioSource source)
        {foreach(var motor in isolatedMotorBeds)if(motor==source)return true;return false;}
        void Update()''')
    return s

prepare('Assets/_Game/Scripts/Campaign/CampaignDirector.cs', director)
prepare('Assets/_Game/Scripts/Puzzle/SlidingDoor.cs', door)
prepare('Assets/_Game/Scripts/Presentation/GameSession.cs', session)
prepare('Assets/_Game/Scripts/Campaign/ReturnRecordingBoundary.cs', boundary)
prepare('Assets/_Game/Scripts/Audio/CampaignRoomTone.cs', tone)
(out / 'integration.patch').write_text(''.join(patches))
(out / 'base-sha256.json').write_text(json.dumps(sources, indent=2)+'\n')
print('Prepared integration.patch and five source drafts. Assets untouched.')
