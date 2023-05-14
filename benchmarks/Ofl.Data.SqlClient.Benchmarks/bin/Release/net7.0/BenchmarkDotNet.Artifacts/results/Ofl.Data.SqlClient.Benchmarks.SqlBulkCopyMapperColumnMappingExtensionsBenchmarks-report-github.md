``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22000.1817/21H2/SunValley)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.202
  [Host]     : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2


```
|                 Method | SequentialAccess | FromDelegate | Columns | Offset | Step |                InputType | Mean | Error | Ratio | RatioSD | Alloc Ratio |
|----------------------- |----------------- |------------- |-------- |------- |----- |------------------------- |-----:|------:|------:|--------:|------------:|
| **Initial_Implementation** |            **False** |        **False** |    **1000** |   **1000** | **1000** |                   **Object** |   **NA** |    **NA** |     **?** |       **?** |           **?** |
|                        |                  |              |         |        |      |                          |      |       |       |         |             |
| **Initial_Implementation** |            **False** |        **False** |    **1000** |   **1000** | **1000** | **ValueTuple&lt;Int32, Int32&gt;** |   **NA** |    **NA** |     **?** |       **?** |           **?** |
|                        |                  |              |         |        |      |                          |      |       |       |         |             |
| **Initial_Implementation** |            **False** |         **True** |    **1000** |   **1000** | **1000** |                   **Object** |   **NA** |    **NA** |     **?** |       **?** |           **?** |
|                        |                  |              |         |        |      |                          |      |       |       |         |             |
| **Initial_Implementation** |            **False** |         **True** |    **1000** |   **1000** | **1000** | **ValueTuple&lt;Int32, Int32&gt;** |   **NA** |    **NA** |     **?** |       **?** |           **?** |
|                        |                  |              |         |        |      |                          |      |       |       |         |             |
| **Initial_Implementation** |             **True** |        **False** |    **1000** |   **1000** | **1000** |                   **Object** |   **NA** |    **NA** |     **?** |       **?** |           **?** |
|                        |                  |              |         |        |      |                          |      |       |       |         |             |
| **Initial_Implementation** |             **True** |        **False** |    **1000** |   **1000** | **1000** | **ValueTuple&lt;Int32, Int32&gt;** |   **NA** |    **NA** |     **?** |       **?** |           **?** |
|                        |                  |              |         |        |      |                          |      |       |       |         |             |
| **Initial_Implementation** |             **True** |         **True** |    **1000** |   **1000** | **1000** |                   **Object** |   **NA** |    **NA** |     **?** |       **?** |           **?** |
|                        |                  |              |         |        |      |                          |      |       |       |         |             |
| **Initial_Implementation** |             **True** |         **True** |    **1000** |   **1000** | **1000** | **ValueTuple&lt;Int32, Int32&gt;** |   **NA** |    **NA** |     **?** |       **?** |           **?** |

Benchmarks with issues:
  SqlBulkCopyMapperColumnMappingExtensionsBenchmarks.Initial_Implementation: DefaultJob [SequentialAccess=False, FromDelegate=False, Columns=1000, Offset=1000, Step=1000, InputType=Object]
  SqlBulkCopyMapperColumnMappingExtensionsBenchmarks.Initial_Implementation: DefaultJob [SequentialAccess=False, FromDelegate=False, Columns=1000, Offset=1000, Step=1000, InputType=ValueTuple<Int32, Int32>]
  SqlBulkCopyMapperColumnMappingExtensionsBenchmarks.Initial_Implementation: DefaultJob [SequentialAccess=False, FromDelegate=True, Columns=1000, Offset=1000, Step=1000, InputType=Object]
  SqlBulkCopyMapperColumnMappingExtensionsBenchmarks.Initial_Implementation: DefaultJob [SequentialAccess=False, FromDelegate=True, Columns=1000, Offset=1000, Step=1000, InputType=ValueTuple<Int32, Int32>]
  SqlBulkCopyMapperColumnMappingExtensionsBenchmarks.Initial_Implementation: DefaultJob [SequentialAccess=True, FromDelegate=False, Columns=1000, Offset=1000, Step=1000, InputType=Object]
  SqlBulkCopyMapperColumnMappingExtensionsBenchmarks.Initial_Implementation: DefaultJob [SequentialAccess=True, FromDelegate=False, Columns=1000, Offset=1000, Step=1000, InputType=ValueTuple<Int32, Int32>]
  SqlBulkCopyMapperColumnMappingExtensionsBenchmarks.Initial_Implementation: DefaultJob [SequentialAccess=True, FromDelegate=True, Columns=1000, Offset=1000, Step=1000, InputType=Object]
  SqlBulkCopyMapperColumnMappingExtensionsBenchmarks.Initial_Implementation: DefaultJob [SequentialAccess=True, FromDelegate=True, Columns=1000, Offset=1000, Step=1000, InputType=ValueTuple<Int32, Int32>]
