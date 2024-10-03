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
| **Game**                                   | **Game Version** | **Status** | **Notes**                                                                                             | **UniTAS Version** |
|--------------------------------------------|------------------|------------|-------------------------------------------------------------------------------------------------------|--------------------|
| [Baldi's Basics in Education and Learning] | 1.4.3            | Broken     | Game freezes on soft restart when running a TAS due to resetting a static field in Rewired            | 0.4.0              |
| [Bubble Jcat]                              | Latest           | Perfect    |                                                                                                       | 0.4.0              |
| [Cat Quest]                                | 1.2.10           | Broken     | Game freezes on soft restart when running a TAS due to resetting a static field in Rewired            | 0.4.0              |
| [Cat Quest II]                             | 1.7.6            | Broken     | NullReferenceException on game start, permanent loading screen                                        | 0.4.0              |
| [Cuphead]                                  | 1.3.2            | Broken     | NullReferenceException on soft restart when running a TAS, permanent black screen                     | 0.4.0              |
| [Deepest Sword]                            | 0.1.5c           | Broken     | ArgumentException when running a TAS, can be fully TAS'd but will crash randomly                      | 0.4.0              |
| [Do It For Me]                             | Latest           | Broken     | Menu button not clickable                                                                             | 0.3.0              |
| [Hololive Isekai]                          | 0.3              | Good       | NullReferenceException shows rarely when running a TAS at an unknown moment in the game               | 0.4.0              |
| [HuniePop]                                 | 1.2.0            | Broken     | Game crashes with a memory access violation on soft restart when running a TAS                        | 0.4.0              |
| [It Steals]                                | Latest           | Perfect    |                                                                                                       | 0.4.0              |
| [Keep Talking and Nobody Explodes]         | 1.9.24           | Broken     | IndexOutOfRangeException and NullReferenceException on soft restart when running a TAS                | 0.4.0              |
| [NEEDY STREAMER OVERLOAD]                  | Latest           | Broken     | Game freezes on soft restart when running a TAS due to resetting a static field in Rewired            | 0.4.0              |
| [PlateUp!]                                 | 1.0.5            | Broken     | Game crashes with IndexOutOfRangeException on soft restart when running a TAS                         | 0.4.0              |
| [Resonance of the Ocean]                   | 1.2.6            | Broken     | Permanent blue screen on soft restart when running a TAS                                              | 0.4.0              |
| [Subnautica]                               | 70086            | Broken     | Permanent black screen on game start                                                                  | 0.4.0              |
| [ULTRAKILL]                                | Latest           | Broken     | Game works but has some exceptions in the log and menu might not be working, but intro skipping works | 0.4.0              |
| [Untitled Goose Game]                      | 1.1.4            | Broken     | Game freezes on soft restart when running a TAS due to resetting a static field in Rewired            | 0.4.0              |
| [YuraYura! Tidying up the tilting tower!]  | 1.0              | Perfect    |                                                                                                       | 0.4.0              |

[Baldi's Basics in Education and Learning]: https://basically-games.itch.io/baldis-basics
[Bubble Jcat]: https://joysugamu.itch.io/bubble-jcat
[Cat Quest]: https://store.steampowered.com/app/593280/Cat_Quest/
[Cat Quest II]: https://store.steampowered.com/app/914710/Cat_Quest_II/
[Cuphead]: https://store.steampowered.com/app/268910/Cuphead/
[Deepest Sword]: https://cosmicadventuresquad.itch.io/deepest-sword
[Do It For Me]: https://lixiangames.itch.io/doitforme
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

