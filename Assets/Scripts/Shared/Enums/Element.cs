using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shared.Enums
{
    public enum Element
    {
        Physical,
        Cryo,
        Hydro,
        Pyro,
        Electro,
        Anemo,
        Geo,
        Dendro,
        None,     // Used as the Heal field in HealthModifyFeedback
        Piercing
    }
    
    [Flags]
    public enum ElementalApplication
    {
        None,
        Cryo    = 1 << Element.Cryo,
        Hydro   = 1 << Element.Hydro,
        Pyro    = 1 << Element.Pyro,
        Electro = 1 << Element.Electro,
        Anemo   = 1 << Element.Anemo,
        Geo     = 1 << Element.Geo,
        Dendro  = 1 << Element.Dendro,
        CryoDendro = Cryo | Dendro
    }
    
    public enum ElementalReaction
    {
        None,
        Melt                = ElementalApplication.Cryo    | ElementalApplication.Pyro,
        Superconduct        = ElementalApplication.Cryo    | ElementalApplication.Electro,
        Frozen              = ElementalApplication.Cryo    | ElementalApplication.Hydro,
        Vaporize            = ElementalApplication.Hydro   | ElementalApplication.Pyro,
        ElectroCharged      = ElementalApplication.Hydro   | ElementalApplication.Electro,
        Bloom               = ElementalApplication.Hydro   | ElementalApplication.Dendro,
        Overloaded          = ElementalApplication.Pyro    | ElementalApplication.Electro,
        Burning             = ElementalApplication.Pyro    | ElementalApplication.Dendro,
        Quicken             = ElementalApplication.Electro | ElementalApplication.Dendro,
        CryoSwirl           = ElementalApplication.Anemo   | ElementalApplication.Cryo,
        HydroSwirl          = ElementalApplication.Anemo   | ElementalApplication.Hydro,
        PyroSwirl           = ElementalApplication.Anemo   | ElementalApplication.Pyro,
        ElectroSwirl        = ElementalApplication.Anemo   | ElementalApplication.Electro,
        CryoCrystallize     = ElementalApplication.Geo     | ElementalApplication.Cryo,
        HydroCrystallize    = ElementalApplication.Geo     | ElementalApplication.Hydro,
        PyroCrystallize     = ElementalApplication.Geo     | ElementalApplication.Pyro,
        ElectroCrystallize  = ElementalApplication.Geo     | ElementalApplication.Electro
    }

    public static class ElementalStatic
    {
        public static readonly Dictionary<ElementalReaction, ValueTuple<Color32, Color32>> ReactionColors 
            = new()
            {
                { ElementalReaction.Melt,           ( new Color32(255, 204, 104, 255),
                                                      new Color32(162, 88 , 15 , 255) ) },
                { ElementalReaction.Superconduct,   ( new Color32(183, 177, 255, 255),
                                                      new Color32(74 , 27 , 211, 255) ) },
                { ElementalReaction.Frozen,         ( new Color32(153, 255, 255, 255),
                                                      new Color32(17 , 164, 253, 255) ) },
            
                { ElementalReaction.Vaporize,       ( new Color32(255, 204, 102, 255),
                                                      new Color32(179, 91 , 38 , 255) ) },
                { ElementalReaction.ElectroCharged, ( new Color32(225, 154, 255, 255),
                                                      new Color32(127, 50 , 218, 255) ) },
                { ElementalReaction.Bloom,          ( new Color32(0  , 234, 83 , 255),
                                                      new Color32(61 , 104, 21 , 255) ) },
            
                { ElementalReaction.Overloaded,     ( new Color32(252, 129, 157, 255),
                                                      new Color32(142, 46 , 80 , 255) ) },
                { ElementalReaction.Burning,        ( new Color32(255, 155, 0  , 255),
                                                      new Color32(135, 62 , 17 , 255) ) },
            
                { ElementalReaction.Quicken,        ( new Color32(0  , 234, 83 , 255),
                                                      new Color32(55 , 101, 9  , 255) ) },
            
                { ElementalReaction.CryoSwirl,      ( new Color32(102, 255, 204, 255),
                                                      new Color32(55 , 105, 105, 255) ) },
                { ElementalReaction.HydroSwirl,     ( new Color32(102, 255, 204, 255),
                                                      new Color32(55 , 105, 105, 255) ) },  
                { ElementalReaction.PyroSwirl,      ( new Color32(102, 255, 204, 255),
                                                      new Color32(55 , 105, 105, 255) ) },
                { ElementalReaction.ElectroSwirl,   ( new Color32(102, 255, 204, 255),
                                                      new Color32(55 , 105, 105, 255) ) },
            
                { ElementalReaction.CryoCrystallize,    ( new Color32(255, 213, 102, 255),
                                                          new Color32(107, 73 , 10 , 255) ) },
                { ElementalReaction.HydroCrystallize,   ( new Color32(255, 213, 102, 255),
                                                          new Color32(107, 73 , 10 , 255) ) },
                { ElementalReaction.PyroCrystallize,    ( new Color32(255, 213, 102, 255),
                                                          new Color32(107, 73 , 10 , 255) ) },
                { ElementalReaction.ElectroCrystallize, ( new Color32(255, 213, 102, 255),
                                                          new Color32(107, 73 , 10 , 255) ) }
            };
        
        public static CostType ToCostType(this Element element)
        {
            return element switch
            {
                Element.Cryo    => CostType.Cryo,
                Element.Hydro   => CostType.Hydro,
                Element.Pyro    => CostType.Pyro,
                Element.Electro => CostType.Electro,
                Element.Geo     => CostType.Geo,
                Element.Dendro  => CostType.Dendro,
                Element.Anemo   => CostType.Anemo,
                _               => CostType.None
            };
        }
        
        public static ElementalApplication ToApplication(this Element element)
        {
            if (element is Element.None or Element.Physical or Element.Piercing)
                return ElementalApplication.None;
            
            return (ElementalApplication)(1 << (int)element);
        }
        
        public static Element ToElement(this ElementalApplication application)
        {
            var result = -1;
            for (var i = (int)application; i != 0; i >>= 1)
                result += 1;
            
            return (Element)result; 
        }
        
        public static ElementalReaction ToReaction(this ElementalApplication applied, ElementalApplication incoming)
            => (ElementalReaction)(applied | incoming);
    }
}