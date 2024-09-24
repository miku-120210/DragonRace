using Fusion;
using UnityEngine;

public enum NormalSkillEnums
{
  FriedChicken = 100,
  HamburgSteak = 200,
  PorkCutlet = 300,
  MeatStirFriedInSweetAndSpicySauce = 400,
}

public struct Skills
{
  public NormalSkillEnums SkillEnum { get; private set; }
  public NetworkString<_16> SkillName { get; private set; }

  public Skills(NormalSkillEnums skillEnum)
  {
    SkillEnum = skillEnum;
    SkillName = skillEnum.ToString();
  }
  
  public static Skills Default => new Skills(NormalSkillEnums.FriedChicken);

  public void UseSkill(PlayerBehaviour player)
  {
    switch (SkillEnum)
    {
      case NormalSkillEnums.FriedChicken:
        Debug.Log("Use Fried Chicken");
        break;
      case NormalSkillEnums.HamburgSteak:
        Debug.Log("Use Hamburg Steak");
        break;
      case NormalSkillEnums.PorkCutlet:
        Debug.Log("Use Pork Cutlet");
        break;
      case NormalSkillEnums.MeatStirFriedInSweetAndSpicySauce:
        Debug.Log("Use Meat Stir-Fried In Sweet And Spicy Sauce");
        break;
    }
  }
}
