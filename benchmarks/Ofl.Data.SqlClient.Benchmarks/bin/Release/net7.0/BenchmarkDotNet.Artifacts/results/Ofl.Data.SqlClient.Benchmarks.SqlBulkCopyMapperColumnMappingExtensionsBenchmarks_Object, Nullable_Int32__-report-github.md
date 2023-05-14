``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22000.1817/21H2/SunValley)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.202
  [Host]     : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2


```
|                 Method | SequentialAccess | FromDelegate | Columns | Rows | Offset | Step |      Mean |    Error |    StdDev | Ratio | Allocated | Alloc Ratio |
|----------------------- |----------------- |------------- |-------- |----- |------- |----- |----------:|---------:|----------:|------:|----------:|------------:|
| **Initial_Implementation** |            **False** |        **False** |    **1000** | **1000** |      **0** |    **1** |  **63.45 ms** | **1.174 ms** |  **1.862 ms** |  **1.00** |      **86 B** |        **1.00** |
|                        |                  |              |         |      |        |      |           |          |           |       |           |             |
| **Initial_Implementation** |            **False** |         **True** |    **1000** | **1000** |      **0** |    **1** | **350.96 ms** | **6.955 ms** | **10.195 ms** |  **1.00** |     **600 B** |        **1.00** |
|                        |                  |              |         |      |        |      |           |          |           |       |           |             |
| **Initial_Implementation** |             **True** |        **False** |    **1000** | **1000** |      **0** |    **1** |  **61.28 ms** | **1.179 ms** |  **1.103 ms** |  **1.00** |      **67 B** |        **1.00** |
|                        |                  |              |         |      |        |      |           |          |           |       |           |             |
| **Initial_Implementation** |             **True** |         **True** |    **1000** | **1000** |      **0** |    **1** | **338.98 ms** | **6.369 ms** |  **8.503 ms** |  **1.00** |     **600 B** |        **1.00** |
