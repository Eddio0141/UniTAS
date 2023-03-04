# Working tiers
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

| Game            | Version | Status  | Notes                                                                                                                                                                            |
|:--------------- |:------- |:-------:|:-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [It Steals]     | Latest  | Perfect |                                                                                                                                                                                  |
| [Do It For Me]  | Latest  | Perfect |                                                                                                                                                                                  |
| [yurayura tutt] | Latest  | Desync  | TAS desyncs due to invoke to `YuraYuraModel.PlayFootStepForReal` and `YuraYuraModel.PlayFootStep` being inconsistent between runs. Stack trace doesn't show what is calling this |
| Hunie Pop       | 1.2.0   | Broken  | Exception upon game start                                                                                                                                                        |
| KTaNE           | ?       | Broken  | Throws an exception on launch                                                                                                                                                    |


[It Steals]: https://store.steampowered.com/app/1349060/It_Steals/
[yurayura tutt]: https://cornflowerblue.itch.io/yurayura-tidying-up-the-tilting-tower
[Do It For Me]: https://lixiangames.itch.io/doitforme
