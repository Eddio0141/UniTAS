# How to manually add a game to the list
- Modify `UniTAS/docs/compatibility-list.json` and add a new entry following the format of the other entries
- If providing the link to the game, make sure the game name is `[Game Name]`, and add the link to the bottom of this file following the format of the other entries
- Use [json-to-markdown](https://tableconvert.com/json-to-markdown) to convert the json to markdown
  - For the `table generator`, select `First row as header` and `Bold first row` and set text align to `Left`
- Replace the table in [Compatibility list](#compatibility-list) with the generated markdown table

# Status tiers
- Perfect
  - Works as UniTAS intends to
- Good
  - No problem TASing, has some unnoticable issues or bugs that doesn't affect the game / TAS
- Desync
  - TAS desyncs, but no major issues
- Broken
  - Game doesn't function as intended, either in TAS playback or normal gameplay
  - This also counts if an exception prevents the game from running
- Crash
  - Game crashes on launch due to UniTAS
  - This is if the game hard crashes

# Compatibility list
| **Game**                                   | **Game Version** | **Status**  | **Notes**                                                                                                                     | **UniTAS Version** |
|--------------------------------------------|------------------|-------------|-------------------------------------------------------------------------------------------------------------------------------|--------------------|
| [Baldi's Basics in Education and Learning] | 1.4.3            | Broken      | Inputs do not work in-game after soft restarting, see #186                                                                    | 0.5.1              |
| [Bubble Jcat]                              | Latest           | Perfect     |                                                                                                                               | 0.5.1              |
| [Cat Quest]                                | 1.2.10           | Good/Broken | NullReferenceException after soft restart related to initializing GOG Galaxy, keyboard input does not work, see #186 and #233 | 0.5.1              |
| [Cat Quest II]                             | 1.7.6            | Broken      | NullReferenceException on game start, permanent loading screen                                                                | 0.5.1              |
| [Cuphead]                                  | 1.3.2            | Broken      | Game freezes on invoking completion event after soft restart, see #11                                                         | 0.5.1              |
| [Deepest Sword]                            | 0.1.5c           | Perfect     |                                                                                                                               | 0.5.1              |
| [Do It For Me]                             | Latest           | Broken      | Menu button not clickable                                                                                                     | 0.3.0              |
| [Doki Doki Literature Club Plus]           | Latest           | Broken      | Coroutine issue on soft restart, keyboard input does not work, see #234                                                       | 0.5.1              |
| [Hololive Isekai]                          | 0.3              | Good        | NullReferenceException shows rarely when running a TAS at an unknown moment in the game                                       | 0.5.1              |
| [HuniePop]                                 | 1.2.0            | Perfect     |                                                                                                                               | 0.5.1              |
| [It Steals]                                | Latest           | Perfect     |                                                                                                                               | 0.5.1              |
| [Keep Talking and Nobody Explodes]         | 1.9.24           | Good        | NullReferenceException on soft restart, see #221                                                                              | 0.5.1              |
| [NEEDY STREAMER OVERLOAD]                  | Latest           | Good/Broken | Keyboard input does not work, see #186                                                                                        | 0.5.1              |
| [PlateUp!]                                 | 1.0.5            | Broken      | ArgumentNullException on soft restart, input does not work, see #14                                                           | 0.5.1              |
| [Resonance of the Ocean]                   | 1.2.6            | Broken      | Permanent blue screen on soft restart when running a TAS, see #206                                                            | 0.5.1              |
| [Subnautica]                               | 70086            | Broken      | NullReferenceException on start and soft restart in-game freezes, see #9 and #186                                             | 0.5.1              |
| [ULTRAKILL]                                | Latest           | Broken      | Game works but has some exceptions in the log and menu might not be working, but intro skipping works                         | 0.4.0              |
| [Untitled Goose Game]                      | 1.1.4            | Broken      | Soft restart breaks GUI, NullReferenceException when clicking the game's own reset button, see #13                            | 0.5.1              |
| [YuraYura! Tidying up the tilting tower!]  | 1.0              | Desync      | Sometimes the TAS will sync (boulders fall in correct place, correct positioning of the maid), sometimes it will not          | 0.5.1              |

[Baldi's Basics in Education and Learning]: https://basically-games.itch.io/baldis-basics
[Bubble Jcat]: https://joysugamu.itch.io/bubble-jcat
[Cat Quest]: https://store.steampowered.com/app/593280/Cat_Quest/
[Cat Quest II]: https://store.steampowered.com/app/914710/Cat_Quest_II/
[Cuphead]: https://store.steampowered.com/app/268910/Cuphead/
[Deepest Sword]: https://cosmicadventuresquad.itch.io/deepest-sword
[Do It For Me]: https://lixiangames.itch.io/doitforme
[Doki Doki Literature Club Plus]: https://store.steampowered.com/app/1388880/Doki_Doki_Literature_Club_Plus/
[Hololive Isekai]: https://drweam.itch.io/hololive-isekai
[HuniePop]: https://store.steampowered.com/app/339800/HuniePop/
[It Steals]: https://store.steampowered.com/app/1349060/It_Steals/
[Keep Talking and Nobody Explodes]: https://store.steampowered.com/app/341800/Keep_Talking_and_Nobody_Explodes/
[NEEDY STREAMER OVERLOAD]: https://store.steampowered.com/app/1451940/NEEDY_STREAMER_OVERLOAD/
[PlateUp!]: https://store.steampowered.com/app/1599600/PlateUp/
[Resonance of the Ocean]: https://uimss.itch.io/resonance-of-the-ocean
[Subnautica]: https://store.steampowered.com/app/264710/Subnautica/
[ULTRAKILL]: https://store.steampowered.com/app/1229490/ULTRAKILL/
[Untitled Goose Game]: https://store.steampowered.com/app/837470/Untitled_Goose_Game/
[YuraYura! Tidying up the tilting tower!]: https://cornflowerblue.itch.io/yurayura-tidying-up-the-tilting-tower

