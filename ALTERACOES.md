# Versão de 1º de novembro de 2026

- Tamanhos 24, 32, 40, 48, 56 e 64 px; padrão de 32 px em configurações novas.
- Lista única para interface, prévia, aplicação, preferências e verificação.
- Preferências antigas de 24, 48 e 64 px preservadas pelo valor em pixels.
- Executável portátil com recursos de cursores e monitor incorporados.
- Monitor de persistência implementado e incluído integralmente no repositório. A nova implementação mantém os formatos de backup/preferências originais, evita instâncias duplicadas por usuário, oferece controle pela bandeja e respeita cursores ocultos.
- Instalador opcional por usuário, que encerra as instâncias antigas da própria conta e atualiza o atalho.
- Testes nativos Windows da interface, imagens, hotspots, dimensões carregadas, leitor do monitor e executável isolado de arquivos externos.

O módulo original do monitor estava indisponível; ele não foi apresentado como se tivesse sido recuperado. A implementação nova está em `CursorPersistence.cs` para revisão.
