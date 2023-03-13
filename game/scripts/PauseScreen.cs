using System;
using System.Text;
using Godot;

public class PauseScreen : WindowDialog
{
    private static readonly string TEXT = @"
- VSauce. ""How To Count Past Infinity"". https://youtu.be/SrU9YDoXE88
  - Watch this video to learn the *correct* Math about various 
    infinities. 
- Germapur. ""Miner Gun Builder"" (MGB). 
  - Play MGB if you want a much longer power-growing arc. 
  - MGB also directly inspired my game. 
- Patrick Traynor. ""Patrick's Parabox"". https://www.patricksparabox.com
  - An awesome puzzle game where infinity is also a necessary 
    consequence of the game mechanics. 

# Alpha & Beta Testers
maguro. AC. Alith. 

# Attribution & Thanks
- Pixel font ""m5x7"" by Daniel Linssen. https://managore.itch.io/m5x7
- Godot 3.5.1 engine. 
- C#. 

";
    public override void _Ready()
    {
        GetNode<Button>("VBox/HBox/ResumeButton").Connect(
            "pressed", this, "Resume"
        );
        GetNode<Button>("VBox/HBox/QuitButton").Connect(
            "pressed", this, "SaveAndQuit"
        );

        StringBuilder sB = new StringBuilder();

        sB.Append(@"
# Recommended
".Substring(1));

        if (GameState.Persistent.MadeInf) {
            sB.Append(@"
- VSauce. ""How To Count Past Infinity"". https://youtu.be/SrU9YDoXE88
  - Watch this video to learn the *correct* Math about various 
    infinities. 
- Patrick Traynor. ""Patrick's Parabox"". https://www.patricksparabox.com
  - An awesome puzzle game where infinity is also a necessary 
    consequence of the game mechanics. 
".Substring(1));
        }

        sB.Append(@"
- Germapur. ""Miner Gun Builder"" (MGB). 
  - Play MGB if you want a much longer power-growing arc. 
  - MGB also directly inspired my game. 

".Substring(1));

        if (GameState.Persistent.MyTypelessGem != null) {
            sB.Append(@"
# The automatic solver of Custom Gem mean effect  
- *Location* (2D) = Int X Int.  
- *Direction* (2D) = Right | Up | Left | Down.  
- A custom gem has one source and one drain. Denote 
  Source = (L, D) where L is Location and D is Direction. 
  Denote Drain = Ll', undefined) where L' is location.  
- *Mana* = Number | Infinities.  
- Each mana *Particle* (leaving a cyan trail) = 
  location X Direction X Mana.  
- Given a circuit, the *System State* is a set of Particles.  
- A *Superposition* is a distribution of System States.  
- Given a Superposition S, function F ""timesteps"" / 
  ""advects"" it into its next-step Superposition F(S). 
  A stochastic gem ""splits"" one Particle into 
  two, each w/ halved mana.  
- Given a Superposition S, we can index it with a 
  (Location X Direction) (L, D) so that S[L, D] is the 
  summed mana of all Particles at the Location L with 
  Direction D. Notice we can write S[Source] and S[Drain].  
- Superposition S is *F-closed* iif for every 
  (Location X Direction) (L, D), F(S)[L, D] <= S[L, D].  
- When evaluating typed (i.e. finite-meta) custom gems, 
  the solver estimates the smallest F-closed 
  Superposition, under the constraint S[Source] = 1.  
- When evaluating the typeless custom gem (TCG), the TCG's
  multiplier M becomes an unknown too. Let the guess 
  G = (S, M), where S is the Superposition and M is the 
  hypothesized TCG multiplier.  
- A guess G = (S, M) is F-closed iif these two conditions are met:  
  - S is F-closed.  
  - S[Drain] <= M.  
- Finally, the solver estimates the smallest f-closed 
  guess G* = (S*, M*). M* is then taken to be the 
  multiplier of the TCG.  

## An example
- Suppose you designed a TCG, where TCG(x) = TCG(1 + x).  
  Then TCG(x) = 0, since we only take the smallest 
  F-closed solution.  
- This may be counter-intuitive, because the TCG looks 
  like it's chaining an infinite number of +1 gems. You 
  can unpack and obtain TCG(x) = TCG(1 + 1 + ... + x), 
  but the outer-most application TCG means the zero 
  solution cannot be rejected.  
- In contrast, if you move the +1 gem to the right and 
  TCG(x) = TCG(x) + 1, then TCG(x) = inf * x.  
".Substring(1));
        }

    sB.Append(@"
# Alpha & Beta Testers
maguro. AC. Alith. 

# Attribution & Thanks
- Pixel font ""m5x7"" by Daniel Linssen. https://managore.itch.io/m5x7
- Godot 3.5.1 engine. 
- C#. 

".Substring(1));

        sB.Append("# Game save \n");
        sB.Append("Location: \n");
        sB.Append(Godot.OS.GetUserDataDir());
        sB.Append("\n");
        sB.Append("Delete all files there to reset the game. \n");
        GetNode<TextEdit>("VBox/TextHBox/TextEdit").Text += sB.ToString();
    }

    public void Resume() {
        Hide();
        Director.UnpauseWorld();
    }
    public void SaveAndQuit() {
        SaveLoad.Save();
        Main.Singleton.Quit();
    }
}
