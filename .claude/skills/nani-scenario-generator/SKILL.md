---
name: nani-scenario-generator
description: Generate Naninovel scenario files (.nani) from natural language character settings and story outlines. Use this skill when the user wants to create visual novel scripts in Unity's Naninovel format, needs to write dialogue scenarios, wants to convert character interactions into .nani scripts, or mentions creating/editing Naninovel scripts, visual novel scenarios, dialogue scripts, or story events for their Unity project.
---

# Naninovel Scenario Generator

This skill converts natural language character settings and story outlines into properly formatted Naninovel scenario files (.nani).

## Understanding the Input

The user will provide character information and a story outline in natural language. Extract:

1. **Character Information**: Names, poses/emotions, any special notes
2. **Story Structure**: Opening scene, character introductions, dialogue sequences, choices/branches, ending
3. **Scene Elements**: Backgrounds, camera movements, sound effects, BGM
4. **Flow**: Labels for branching (Rescue, Watch, Accept, etc.)

## Naninovel Script Syntax

### Comments
```
; Comment text - Use for section headers and notes
```

### Labels (for branching)
```
# LabelName
```

### Commands
```
@back BackgroundName time:1.0
@char CharacterName.pose pose:center time:0.6
@hide CharacterName
@hideAll time:1
@wait 0.5
@camera zoom:0.3
@choice "Option text" goto:.LabelName
@goto .LabelName
@bgm BGMName volume:0.3
@stopBgm BGMName fade:1
@sfx SFXName
@spawn SpawnName pos:55,45
@showUI time:1
@hideUI
@shake MainBackground hor:true ver:false power:0.2
@delay 0.5
@end
```

### Dialogue
```
CharacterName: Dialogue text here
```

### Narration (no character name)
```
Plain text describes the scene or action.
```

### Character Poses (common ones)
- default, scared, nervous, angry, happy, determined, love, neutral

### Scene Positions
- center, left, right

## Converting Natural Language to .nani Format

### Character Appearances
When a character is mentioned:
- Use `@char Name.pose pose:position` to show them
- Include their emotion/pose based on context (scared, happy, angry, etc.)
- Use `@hide Name` when they leave the scene

### Backgrounds
- Extract scene location descriptions to `@back` commands
- Add `time:1` or appropriate duration for transitions

### Dialogue
- Convert each line of dialogue to `Character: message` format
- Keep narration as plain text without a character prefix
- For unnamed speakers, use `Unknown: message`

### Choices and Branching
- When multiple options are presented, create `@choice` commands
- Create labels (`# LabelName`) for each branch
- Use `@goto` to return to shared paths

### Camera and Effects
- Add `@camera zoom:X` for dramatic moments
- Include `@shake` for impact/intense scenes
- Use `@wait` and `@delay` for pacing

### Sound
- Add `@bgm` for background music at scene start
- Use `@sfx` for sound effects (actions, impacts)
- `@stopBgm` with fade for music transitions

## Output Format

1. Start with a comment describing the episode
2. Set initial background and UI
3. Introduce characters with appropriate poses
4. Write dialogue and narration in sequence
5. Handle branching with labels and choices
6. End with `@hideAll`, `@wait 1.1`, and `@end`

## File Saving

Save the generated .nani file to `Assets/Scenario/` with the filename:
- `{character_name}_{episode_number}.nani` (e.g., `elena_1.nani`, `ariel_2.nani`)
- Use `prologue.nani` for prologue/epilogue

If the user specifies a different path, use that instead.

## Examples of Conversion

### Natural Language Input:
"Scene starts in a dark dungeon. Elena is scared, cornered by slimes. She screams for help. The player can choose to help her immediately or watch first."

### Naninovel Output:
```
; Elena story 1
@back Dungeon0 time:1
@showUI time:1
@bgm Action2 volume:.3

@char Elena.scared pose:center
Unknown: 으악! 안 돼! 이건 졸업 기념으로 맞춘 특제 로브란 말이야!

@char Slime pose:left
Unknown: 저리 가! 이렇게 붙으면 마법을 못쓴다고!

@choice "구해준다." goto:.Rescue
@choice "잠깐 지켜본다." goto:.Watch
@stop

# Rescue
나는 검을 뽑아 들고 슬라임 사이로 뛰어들었다.
@hide Elena
@char Slime pose:center
@camera zoom:.3
@delay .5
    @spawn SwordSlashThinBlue pos:55,45
    @sfx Skill_Knife_Throw_B
@delay 1.2
    @hide Slime lazy:true
    @sfx Weapon_Impact_Blood
@wait 1.2
단순한 몬스터였기에 내 검 한 번에 슬라임들은 터져 나갔다.
```

## Notes

- Use consistent character names throughout the script
- Include appropriate wait times between commands for natural flow
- Add comments to separate sections (use `; --- (section) ---` format)
- When creating new characters, assume they follow the same pose conventions
- Preserve the original language (Korean in this project's examples)
