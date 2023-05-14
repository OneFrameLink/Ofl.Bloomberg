``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22000.1817/21H2/SunValley)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.202
  [Host]     : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2


```
|                 Method | SequentialAccess | FromDelegate | Columns | Rows | Offset | Step |      Mean |    Error |   StdDev | Ratio | Allocated | Alloc Ratio |
|----------------------- |----------------- |------------- |-------- |----- |------- |----- |----------:|---------:|---------:|------:|----------:|------------:|
| **Initial_Implementation** |            **False** |        **False** |    **1000** | **1000** |      **0** |    **1** |  **49.33 ms** | **0.745 ms** | **0.660 ms** |  **1.00** |      **67 B** |        **1.00** |
|                        |                  |              |         |      |        |      |           |          |          |       |           |             |
| **Initial_Implementation** |            **False** |         **True** |    **1000** | **1000** |      **0** |    **1** | **120.80 ms** | **2.414 ms** | **3.462 ms** |  **1.00** |     **120 B** |        **1.00** |
|                        |                  |              |         |      |        |      |           |          |          |       |           |             |
| **Initial_Implementation** |             **True** |        **False** |    **1000** | **1000** |      **0** |    **1** |  **45.13 ms** | **0.801 ms** | **0.954 ms** |  **1.00** |      **50 B** |        **1.00** |
|                        |                  |              |         |      |        |      |           |          |          |       |           |             |
| **Initial_Implementation** |             **True** |         **True** |    **1000** | **1000** |      **0** |    **1** | **113.81 ms** | **2.181 ms** | **1.821 ms** |  **1.00** |     **120 B** |        **1.00** |
