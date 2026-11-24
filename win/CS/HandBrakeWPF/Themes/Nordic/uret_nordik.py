"""
Fork: Nordik tema kontrol sözlüklerini üretir.

HandBrake'in koyu/açık kontrol sözlüklerinde (Themes/Controls/*.xaml) sabit yazılmış renkler var; palet
anahtarlarını değiştirmek onları etkilemez. Bu betik o dosyaları renk eşlemesiyle Themes/Nordic/ altına
kopyalar ve iki temanın düğme şablonunu yazar.

Ne zaman çalıştırılır: upstream HandBrake güncellenip Themes/Controls değişince, ya da aşağıdaki
renkler değiştirilince.  Kullanım:  python uret_nordik.py   (sonra projeyi yeniden derle)
"""

import re
from pathlib import Path

HERE = Path(__file__).resolve().parent
CONTROLS = HERE.parent / "Controls"

# Nordik Kış paleti (D:\Projects\tasarim-temalari\nordik-kis-neumorfik\TEMA.md)
DARK_MAP = {
    # ComboBoxDark
    '"Black"': '"#1A2029"', '"White"': '"#DCE3EC"', '"WhiteSmoke"': '"#DCE3EC"',
    "#569DE5": "#83A6C2", "#2164A8": "#52759A", "#A8A8A8": "#8A97A8", "#7EB4EA": "#A9C3D8",
    "#B7B7B7": "#9AA6B5", "#606060": "#3A4658", "#4B77A3": "#52759A", "#BFBFBF": "#B8C3D1",
    "#43474C": "#2F3846", "#414141": "#2A3341", "#282828": "#1E2530", "#1D5791": "#3F5F80",
    "#5D5E60": "#4A5668", "#FFFFFFFF": "#FFDCE3EC",
    # ContextMenuDark / TreeviewDark
    "#363636": "#222A35", "#3D26A0DA": "#3D83A6C2", "#FF26A0DA": "#FF83A6C2", "#ACACAC": "#8A97A8",
    "#FF707070": "#FF4A5668", "#FFDADADA": "#FF3A4658",
    # ToolbarDark
    "#AAA": "#5C6879",
    # ProgressBar
    "#FF06B025": "#FF83A6C2", "#FFBCBCBC": "#FF3A4658", "#FFE6E6E6": "#FF2A3341",
}

LIGHT_MAP = {
    # ContextMenuLight
    "#3D26A0DA": "#3D52759A", "#FF26A0DA": "#FF52759A", "#FFF0F0F0": "#FFE6EBF1", "#FFD7D7D7": "#FFD3DAE4",
    "#FF999999": "#FF848C97", "#FF212121": "#FF2E3846", "#3DDADADA": "#3DB4C0CF", "#FFDADADA": "#FFD3DAE4",
    "#FF707070": "#FF7A8796", '"White"': '"#EDF1F6"',
    # ToolbarLight
    "#AAA": "#B4C0CF",
    # ProgressBar
    "#FF06B025": "#FF52759A", "#FFBCBCBC": "#FFB4C0CF", "#FFE6E6E6": "#FFD3DAE4",
}

JOBS = [
    ("ComboBoxDark.xaml", "ComboBoxNordicDark.xaml", DARK_MAP),
    ("ContextMenuDark.xaml", "ContextMenuNordicDark.xaml", DARK_MAP),
    ("ToolbarDark.xaml", "ToolbarNordicDark.xaml", DARK_MAP),
    ("GroupBoxDark.xaml", "GroupBoxNordicDark.xaml", DARK_MAP),
    ("TreeviewDark.xaml", "TreeviewNordicDark.xaml", DARK_MAP),
    ("ProgressBarBoth.xaml", "ProgressBarNordicDark.xaml", DARK_MAP),
    ("ContextMenuLight.xaml", "ContextMenuNordicLight.xaml", LIGHT_MAP),
    ("ToolbarLight.xaml", "ToolbarNordicLight.xaml", LIGHT_MAP),
    ("ProgressBarBoth.xaml", "ProgressBarNordicLight.xaml", LIGHT_MAP),
]

TOKEN = re.compile(r'"(?:Black|White|WhiteSmoke)"|#[0-9A-Fa-f]{3,8}\b')

HEADER = "<!-- Fork: uret_nordik.py tarafından {src} dosyasından üretildi; elle düzenleme, betiği değiştir. -->\n"


def convert(text: str, cmap: dict) -> tuple[str, set]:
    upper = {k.upper(): v for k, v in cmap.items()}
    missed = set()

    def repl(m):
        tok = m.group(0)
        new = upper.get(tok.upper())
        if new is None:
            if tok.startswith("#") and tok.upper() not in ("#00000000",) and not tok.upper().startswith(("#0A", "#21", "#71")):
                missed.add(tok)
            return tok
        return new

    return TOKEN.sub(repl, text), missed


# İki temanın düğme şablonu: yuvarlak köşe, yumuşak gölge, basılıyken gömük (gölgesiz, koyu yüzey).
BUTTON = """<!-- Fork: uret_nordik.py tarafından üretildi. Nordik düğme şablonu ({name}). -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <SolidColorBrush x:Key="Button.Static.Border" Color="{border}" />
    <SolidColorBrush x:Key="Button.Disabled.Border" Color="{border}" />
    <SolidColorBrush x:Key="Button.Disabled.Foreground" Color="{disabled_fg}" />

    <Style TargetType="{{x:Type Button}}">
        <Setter Property="Background" Value="{{DynamicResource Ui.Light}}" />
        <Setter Property="BorderBrush" Value="{{StaticResource Button.Static.Border}}" />
        <Setter Property="Foreground" Value="{{DynamicResource {{x:Static SystemColors.ControlTextBrushKey}}}}" />
        <Setter Property="BorderThickness" Value="1" />
        <Setter Property="HorizontalContentAlignment" Value="Center" />
        <Setter Property="VerticalContentAlignment" Value="Center" />
        <Setter Property="Padding" Value="10,3" />
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="{{x:Type ButtonBase}}">
                    <Border x:Name="border" CornerRadius="8"
                            BorderThickness="{{TemplateBinding BorderThickness}}"
                            Background="{{TemplateBinding Background}}"
                            BorderBrush="{{TemplateBinding BorderBrush}}"
                            SnapsToDevicePixels="true">
                        <Border.Effect>
                            <DropShadowEffect x:Name="shadow" Color="{shadow}" Opacity="{shadow_opacity}" BlurRadius="8" ShadowDepth="2" Direction="315" />
                        </Border.Effect>
                        <ContentPresenter x:Name="contentPresenter" RecognizesAccessKey="True"
                                          Margin="{{TemplateBinding Padding}}"
                                          SnapsToDevicePixels="{{TemplateBinding SnapsToDevicePixels}}"
                                          HorizontalAlignment="{{TemplateBinding HorizontalContentAlignment}}"
                                          VerticalAlignment="{{TemplateBinding VerticalContentAlignment}}"
                                          Focusable="False" />
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="Button.IsDefaulted" Value="true">
                            <Setter Property="BorderBrush" Value="{primary}" TargetName="border" />
                        </Trigger>
                        <Trigger Property="IsMouseOver" Value="true">
                            <Setter Property="BorderBrush" Value="{primary}" TargetName="border" />
                        </Trigger>
                        <Trigger Property="IsPressed" Value="true">
                            <Setter Property="Background" Value="{pressed}" TargetName="border" />
                            <Setter Property="Effect" Value="{{x:Null}}" TargetName="border" />
                        </Trigger>
                        <Trigger Property="IsEnabled" Value="false">
                            <Setter Property="Effect" Value="{{x:Null}}" TargetName="border" />
                            <Setter Property="Opacity" Value="0.55" TargetName="border" />
                            <Setter Property="TextElement.Foreground" Value="{{StaticResource Button.Disabled.Foreground}}" TargetName="contentPresenter" />
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</ResourceDictionary>
"""

BUTTONS = {
    "ButtonNordicLight.xaml": dict(name="açık", border="#C9D2DE", disabled_fg="#848C97", shadow="#9AA8BA",
                                   shadow_opacity="0.45", primary="#52759A", pressed="#D3DAE4"),
    "ButtonNordicDark.xaml": dict(name="koyu", border="#3A4658", disabled_fg="#7A8796", shadow="#0B0E13",
                                  shadow_opacity="0.6", primary="#83A6C2", pressed="#1A2029"),
}


def main():
    for src, dst, cmap in JOBS:
        text = (CONTROLS / src).read_text(encoding="utf-8-sig")
        out, missed = convert(text, cmap)
        (HERE / dst).write_text(HEADER.format(src=src) + out, encoding="utf-8")
        print(f"{dst:32} <- {src}" + (f"   eşlenmeyen: {sorted(missed)}" if missed else ""))
    for dst, vals in BUTTONS.items():
        (HERE / dst).write_text(BUTTON.format(**vals), encoding="utf-8")
        print(f"{dst:32} (şablon)")


if __name__ == "__main__":
    main()
