# Cursor Cores — ajuste de tamanhos para Windows

Atualização do projeto **Cursor Cores**, com 13 formatos, cores personalizadas e uma escala de tamanhos mais gradual.

O seletor anterior pulava de **24 para 48 px**, dobrando o tamanho. Agora oferece **24, 32, 40, 48, 56 e 64 px**. Configurações novas começam em **32 px**; preferências salvas anteriormente continuam com o valor exato escolhido.

## Imagens

![Interface WinForms em teste](docs/imagens/interface-windows-teste.png)

![Comparação dos seis tamanhos](docs/imagens/comparacao-tamanhos.png)

As imagens são geradas pelo próprio código C#/WinForms durante os testes. A aparência das bordas da janela depende do tema do Windows.

## Atualizar seu projeto Windows existente

Este repositório contém os fontes revisados e um atualizador para sua pasta original. O monitor de persistência original (`CursorPersistence.cs`) e o compilador original (`compilar.ps1`) devem estar nessa pasta. Eles não foram recuperados do Drive nesta sessão. Por isso, **não é distribuído aqui um novo executável de produção**.

1. Baixe e extraia este repositório.
2. Feche a janela Cursor Cores e encerre o monitor pelo ícone da bandeja.
3. No PowerShell, dentro da pasta extraída, execute:

```powershell
.\Atualizar.ps1 -PastaProjeto 'C:\Projetos\Cursor Cores'
```

Troque o caminho pela pasta original, que contém `CursorCores.cs`, `PointerStyles.cs`, `CursorPersistence.cs`, `compilar.ps1` e `Base`.

O atualizador faz backup, modifica os trechos de seleção de tamanho e usa o compilador que já existe no seu projeto. Ele preserva os outros trechos do programa e o monitor. Em caso de falha, restaura os fontes e o executável anterior. Depois, abra o novo `Cursor Cores.exe` e experimente **32 ou 40 px**.

Para revisar o ajuste antes de aplicá-lo, veja [CursorCores.cs](CursorCores.cs) e [ALTERACOES.md](ALTERACOES.md). Os arquivos C# na raiz representam a cópia do projeto disponível para esta atualização; o script aplica o ajuste diretamente ao fonte existente para preservar outras mudanças que você já tenha feito.

## O que foi alterado

- Uma lista única com os seis tamanhos evita divergências entre prévia, aplicação e preferências.
- A prévia continua usando o mesmo renderizador do ponteiro aplicado.
- Preferências são restauradas pelo número de pixels, preservando os valores antigos de 24, 48 e 64 px.
- A verificação de cursores contempla os seis tamanhos, inclusive os intermediários.
- Os desenhos, cores e a lógica de persistência do projeto original permanecem sob controle dos fontes originais.

O tamanho indicado é a tela da imagem do cursor, incluindo a margem transparente. O desenho visível ocupa parte dessa tela; 32 e 40 px oferecem opções intermediárias sem mudar as proporções dos formatos.

## Testes

No Windows com .NET Framework 4:

```powershell
.\tests\Testar.ps1
```

Os testes verificam as seis opções, o padrão de 32 px, a restauração das preferências, os 13 formatos em três cores e as dimensões/hotspots dos cursores estáticos e dos quadros animados. No Windows também usam o carregador nativo de cursores. Não alteram o cursor ativo nem as configurações do usuário.

`tests/PersistenceTestStub.cs` é usado **somente** no executável de testes. Ele lança erro se alguém tentar executar funções do monitor. Nunca deve substituir `CursorPersistence.cs` nem ser usado para compilar o aplicativo de produção.

## Base visual

Os arquivos em `Base/` pertencem ao **Bibata Modern Amber v2.0.6**, de [ful1e5](https://github.com/ful1e5/Bibata_Cursor), com licença [GPL-3.0](LICENSE-Bibata). Os desenhos personalizados foram preservados de `PointerStyles.cs` do projeto Cursor Cores do usuário.
