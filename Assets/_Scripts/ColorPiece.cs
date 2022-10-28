using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPiece : MonoBehaviour
{
    public enum ColorType
    {
        YELLOW, PURPLE, RED, BLUE, GREEN, ORANGE, ANY, COUNT, 
    }

    [System.Serializable]
    public struct ColorSprite
    {
        public ColorType color;
        public Sprite sprite;
    }

    public ColorSprite[] _colorSprites;

    private ColorType color;
    public ColorType Color
    {
        get { return color; }
        set { SetColor(value); }
    }

    public int NumColors
    {
        // returns the number of colors we have specified. 
        get { return _colorSprites.Length; }
    }
    private Sprite pieceSprite; 

    private Dictionary<ColorType, Sprite> colorSpriteDictionary;

    private void Awake()
    {
        colorSpriteDictionary = new Dictionary<ColorType, Sprite>();
        for (int i = 0; i < _colorSprites.Length; i++)
        {
            if (!colorSpriteDictionary.ContainsKey(_colorSprites[i].color))
            {
                colorSpriteDictionary.Add(_colorSprites[i].color, _colorSprites[i].sprite);
            }
        }
    }

    public void SetColor(ColorType newColor)
    {
        color = newColor;
        if (colorSpriteDictionary.ContainsKey(newColor))
        {
            //print(colorSpriteDictionary[newColor]); 
            pieceSprite = colorSpriteDictionary[newColor]; 
        }
    }

    public Sprite GetReferencedColorSprite()
    {
        return pieceSprite; 
    }
   
}
