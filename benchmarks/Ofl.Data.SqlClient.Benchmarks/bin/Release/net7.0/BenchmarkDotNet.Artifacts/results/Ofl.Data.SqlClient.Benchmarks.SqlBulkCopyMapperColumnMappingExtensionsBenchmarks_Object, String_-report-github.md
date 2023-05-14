``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22000.1817/21H2/SunValley)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.202
  [Host]     : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2


```
|                 Method | SequentialAccess | FromDelegate | Columns | Rows | Offset | Step |     Mean |    Error |   StdDev | Ratio | Allocated | Alloc Ratio |
|----------------------- |----------------- |------------- |-------- |----- |------- |----- |---------:|---------:|---------:|------:|----------:|------------:|
| **Initial_Implementation** |            **False** |        **False** |    **1000** | **1000** |      **0** |    **1** | **28.03 ms** | **0.870 ms** | **2.524 ms** |  **1.00** |      **19 B** |        **1.00** |
|                        |                  |              |         |      |        |      |          |          |          |       |           |             |
| **Initial_Implementation** |            **False** |         **True** |    **1000** | **1000** |      **0** |    **1** | **33.42 ms** | **0.604 ms** | **0.905 ms** |  **1.00** |      **40 B** |        **1.00** |
|                        |                  |              |         |      |        |      |          |          |          |       |           |             |
| **Initial_Implementation** |             **True** |        **False** |    **1000** | **1000** |      **0** |    **1** | **23.29 ms** | **0.450 ms** | **0.442 ms** |  **1.00** |      **19 B** |        **1.00** |
|                        |                  |              |         |      |        |      |          |          |          |       |           |             |
| **Initial_Implementation** |             **True** |         **True** |    **1000** | **1000** |      **0** |    **1** | **29.45 ms** | **0.589 ms** | **1.411 ms** |  **1.00** |      **40 B** |        **1.00** |
