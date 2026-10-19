# Formulários e validação

Os inputs da lib derivam de `InputBase<T>`: expõem `Value`, `ValueChanged`, `ValueExpression`,
`Disabled`, `Invalid`, `StyleClass` e participam automaticamente de um `EditForm` (via
`EditContext` em cascata).

## Binding básico

```razor
<OpInputText @bind-Value="_name" Placeholder="Nome" Fluid="true" />
<OpTextarea @bind-Value="_notes" Rows="4" AutoResize="true" />
<OpInputNumber @bind-Value="_quantity" Min="1" Max="99" ShowButtons="true" />
<OpSelect TValue="City" @bind-Value="_city" Options="Cities" OptionLabel="Name" ShowClear="true" />
<OpCheckbox @bind-Value="_accepted" />
<OpDatePicker @bind-Value="_date" ShowIcon="true" />

@code {
    private string? _name;
    private string? _notes;
    private decimal? _quantity;
    private City? _city;
    private bool? _accepted;
    private DateTime? _date;

    private sealed record City(string Name, string Code);

    private static readonly object[] Cities =
    [
        new City("New York", "NY"),
        new City("Rome", "RM"),
        new City("London", "LDN")
    ];
}
```

Detalhes que costumam gerar erro de compilação:

- `OpSelect<TValue>` exige `TValue` explícito quando a lista é de `object[]`.
- `OpInputNumber` usa `decimal?`; `OpDatePicker` usa `DateTime?`; `OpCheckbox` usa `bool?`.
- `Options` é `IReadOnlyList<object>`; `OptionLabel`/`OptionValue` são nomes de propriedades
  do item (não expressões).

## EditForm + DataAnnotations

```razor
<EditForm Model="_form" OnValidSubmit="Submit">
    <DataAnnotationsValidator />

    <label for="name">Nome</label>
    <OpInputText Id="name" @bind-Value="_form.Name" Fluid="true" />
    <ValidationMessage For="() => _form.Name" />

    <label for="email">Email</label>
    <OpInputText Id="email" @bind-Value="_form.Email" Fluid="true" />
    <ValidationMessage For="() => _form.Email" />

    <label for="birth">Nascimento</label>
    <OpDatePicker Id="birth" @bind-Value="_form.BirthDate" ShowIcon="true" />
    <ValidationMessage For="() => _form.BirthDate" />

    <OpButton Type="submit" Label="Enviar" />
</EditForm>

@code {
    private readonly FormModel _form = new();

    private void Submit()
    {
        // _form está válido
    }

    private sealed class FormModel
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(80, ErrorMessage = "Use no máximo 80 caracteres.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "A data é obrigatória.")]
        public DateTime? BirthDate { get; set; }
    }
}
```

- O estado `Invalid` do input é ativado pela validação do `EditForm`; também dá para forçar
  com `Invalid="true"`.
- `DisplayName` define o nome usado nas mensagens quando não há `[Display]`.
- `IgnoreEditContext="true"` desliga a participação no `EditContext` (útil para campos que
  não pertencem ao modelo validado).
- Use `ValidationMessage For="() => _form.Prop"` ou `ValidationSummary`.

## Fluxo típico de CRUD em formulário

```razor
<EditForm Model="_form" OnValidSubmit="Save">
    <DataAnnotationsValidator />
    <OpInputText @bind-Value="_form.Name" Placeholder="Nome" />
    <OpSelect TValue="string" @bind-Value="_form.Category" Options="Categories"
              Placeholder="Categoria" ShowClear="true" />
    <OpButton Type="submit" Label="Salvar" Loading="_saving" />
</EditForm>

@code {
    private FormModel _form = new();
    private bool _saving;

    private static readonly object[] Categories = ["Bebidas", "Padaria", "Limpeza"];

    private async Task Save()
    {
        _saving = true;
        try
        {
            // chamar a API
        }
        finally
        {
            _saving = false;
        }
    }
}
```

## Componentes de entrada mais usados

| Necessidade | Componente |
| --- | --- |
| Texto curto | `OpInputText` |
| Texto longo | `OpTextarea` |
| Número/decimal | `OpInputNumber` |
| Seleção única | `OpSelect<TValue>`, `OpListbox<TValue>` |
| Múltipla seleção | `OpMultiSelect<TValue>` |
| Sim/não | `OpCheckbox`, `OpToggleSwitch` |
| Data/hora | `OpDatePicker` |
| Senha | `OpPassword` |
| Máscara | `OpInputMask`, `OpKeyFilter` |
| Busca com sugestões | `OpAutoComplete<TValue>` |

Confirme os parâmetros de cada um no manifesto (`llms/manifest.json`) antes de usar opções
menos comuns.
