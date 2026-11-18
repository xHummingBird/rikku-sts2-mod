using BaseLib.Abstracts;
using Rikku.RikkuCode.Extensions;
using Godot;

namespace Rikku.RikkuCode.Character;

public class RikkuPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => Rikku.Color;


    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}