// Somente para os testes. Nunca usar no executável de produção.
using System;
partial class CursorCores {
 static void StartGuard(){throw new InvalidOperationException("Monitor ausente no teste.");}
 static void StopGuard(){throw new InvalidOperationException("Monitor ausente no teste.");}
 static void RunGuard(){throw new InvalidOperationException("Monitor ausente no teste.");}
 static void TestPersistence(){throw new InvalidOperationException("Monitor ausente no teste.");}
}
