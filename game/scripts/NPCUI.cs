public class NPCUI : JustOneSpriteSpawnableUI
{
    public NPCUI(NPC npc) : base(npc) {
        MySprite.Texture = npc.Texture();
    }
}
