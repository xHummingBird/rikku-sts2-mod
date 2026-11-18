using MegaCrit.Sts2.Core.Commands;

namespace Rikku.RikkuCode.Extensions;

public static class AudioHelper
{
    private static readonly Random rng = new();

    // =========================
    // ENGLISH
    // =========================

    private static readonly string[] attackSfx =
    {
        "res://Rikku/sounds/attack (1).wav",
        "res://Rikku/sounds/attack (2).wav",
        "res://Rikku/sounds/attack (3).wav"
    };

    private static readonly string[] damagedSfx =
    {
        "res://Rikku/sounds/hit.wav",
        "res://Rikku/sounds/hit_high.wav"
    };

    private static readonly string[] highDamagedSfx =
    {
        "res://Rikku/sounds/hit_high.wav"
    };

    private static readonly string[] criticalDamagedSfx =
    {
        "res://Rikku/sounds/hit_critical.wav"
    };

    private static readonly string[] victorySfx =
    {
        "res://Rikku/sounds/victory (1).wav",
        "res://Rikku/sounds/victory (2).wav"
    };

    private static readonly string[] gameoverSfx =
    {
        "res://Rikku/sounds/gameover (1).wav",
        "res://Rikku/sounds/gameover (2).wav",
        "res://Rikku/sounds/gameover (3).wav"
    };

    private static readonly string[] mixSfx =
    {
        "res://Rikku/sounds/mix (1).wav",
        "res://Rikku/sounds/mix (2).wav",
        "res://Rikku/sounds/mix (3).wav"
    };

    private static readonly string[] iceSfx =
    {
        "res://Rikku/sounds/ice (1).wav",
        "res://Rikku/sounds/ice (2).wav",
        "res://Rikku/sounds/ice (3).wav"
    };

    private static readonly string[] fireSfx =
    {
        "res://Rikku/sounds/fire (1).wav",
        "res://Rikku/sounds/fire (2).wav",
        "res://Rikku/sounds/fire (3).wav"
    };

    private static readonly string[] waterSfx =
    {
        "res://Rikku/sounds/water (1).wav",
        "res://Rikku/sounds/water (2).wav",
        "res://Rikku/sounds/water (3).wav"
    };

    private static readonly string[] thunderSfx =
    {
        "res://Rikku/sounds/thunder (1).wav",
        "res://Rikku/sounds/thunder (2).wav",
        "res://Rikku/sounds/thunder (3).wav"
    };

    private static readonly string[] stealSfx =
    {
        "res://Rikku/sounds/steal (1).wav",
        "res://Rikku/sounds/steal (2).wav",
        "res://Rikku/sounds/steal (3).wav",
        "res://Rikku/sounds/steal (4).wav",
        "res://Rikku/sounds/steal (5).wav",
        "res://Rikku/sounds/steal (6).wav"
    };


    // =========================
    // JAPANESE
    // =========================

    private static readonly string[] attackSfxJap =
    {
        "res://Rikku/sounds/jap/attack (1).wav",
        "res://Rikku/sounds/jap/attack (2).wav",
        "res://Rikku/sounds/jap/attack (3).wav",
        "res://Rikku/sounds/jap/attack (4).wav"
    };

    private static readonly string[] damagedSfxJap =
    {
        "res://Rikku/sounds/jap/hit_1.wav",
        "res://Rikku/sounds/jap/hit_2.wav"
    };

    private static readonly string[] highDamagedSfxJap =
    {
        "res://Rikku/sounds/jap/hit_high.wav"
    };

    private static readonly string[] criticalDamagedSfxJap =
    {
        "res://Rikku/sounds/jap/hit_critical.wav"
    };

    private static readonly string[] victorySfxJap =
    {
        "res://Rikku/sounds/jap/victory (1).wav",
        "res://Rikku/sounds/jap/victory (2).wav"
    };

    private static readonly string[] gameoverSfxJap =
    {
        "res://Rikku/sounds/jap/gameover (1).wav",
        "res://Rikku/sounds/jap/gameover (2).wav",
        "res://Rikku/sounds/jap/gameover (3).wav"
    };

    private static readonly string[] mixSfxJap =
    {
        "res://Rikku/sounds/jap/mix (1).wav",
        "res://Rikku/sounds/jap/mix (2).wav",
        "res://Rikku/sounds/jap/mix (3).wav",
        "res://Rikku/sounds/jap/mix (4).wav",
        "res://Rikku/sounds/jap/mix (5).wav"
    };

    private static readonly string[] iceSfxJap =
    {
        "res://Rikku/sounds/jap/ice (1).wav",
        "res://Rikku/sounds/jap/ice (2).wav",
        "res://Rikku/sounds/jap/ice (3).wav"
    };

    private static readonly string[] fireSfxJap =
    {
        "res://Rikku/sounds/jap/fire (1).wav",
        "res://Rikku/sounds/jap/fire (2).wav",
        "res://Rikku/sounds/jap/fire (3).wav"
    };

    private static readonly string[] waterSfxJap =
    {
        "res://Rikku/sounds/jap/water (1).wav",
        "res://Rikku/sounds/jap/water (2).wav",
        "res://Rikku/sounds/jap/water (3).wav"
    };

    private static readonly string[] thunderSfxJap =
    {
        "res://Rikku/sounds/jap/thunder (1).wav",
        "res://Rikku/sounds/jap/thunder (2).wav",
        "res://Rikku/sounds/jap/thunder (3).wav"
    };

    private static readonly string[] stealSfxJap =
    {
        "res://Rikku/sounds/jap/steal (1).wav",
        "res://Rikku/sounds/jap/steal (2).wav",
        "res://Rikku/sounds/jap/steal (3).wav",
        "res://Rikku/sounds/jap/steal (4).wav",
        "res://Rikku/sounds/jap/steal (5).wav",
        "res://Rikku/sounds/jap/steal (6).wav"
    };


    // =========================
    // PLAY METHODS
    // =========================

    public static void PlayRandomAttack()
    {
        PlayRandomByLanguage(attackSfx, attackSfxJap);
    }

    public static void PlayRandomMix()
    {
        PlayRandomByLanguage(mixSfx, mixSfxJap);
    }

    public static void PlayRandomDamaged()
    {
        PlayRandomByLanguage(damagedSfx, damagedSfxJap);
    }

    public static void PlayRandomDamagedHigh()
    {
        PlayRandomByLanguage(highDamagedSfx, highDamagedSfxJap);
    }

    public static void PlayRandomDamagedCritical()
    {
        PlayRandomByLanguage(criticalDamagedSfx, criticalDamagedSfxJap);
    }

    public static void PlayRandomVictory()
    {
        PlayRandomByLanguage(victorySfx, victorySfxJap);
    }

    public static void PlayRandomGameover()
    {
        PlayRandomByLanguage(gameoverSfx, gameoverSfxJap);
    }

    public static void PlayRandomIce()
    {
        PlayRandomByLanguage(iceSfx, iceSfxJap);
    }

    public static void PlayRandomFire()
    {
        PlayRandomByLanguage(fireSfx, fireSfxJap);
    }

    public static void PlayRandomWater()
    {
        PlayRandomByLanguage(waterSfx, waterSfxJap);
    }

    public static void PlayRandomThunder()
    {
        PlayRandomByLanguage(thunderSfx, thunderSfxJap);
    }

    public static void PlayRandomSteal()
    {
        PlayRandomByLanguage(stealSfx, stealSfxJap);
    }


    // =========================
    // LANGUAGE SELECTION
    // =========================

    private static void PlayRandomByLanguage(
        string[] englishPool,
        string[] japanesePool)
    {
        var pool = RikkuConfig.Voice switch
        {
            VoiceLanguage.Japanese => japanesePool,
            _ => englishPool
        };

        PlayRandom(pool);
    }

    private static void PlayRandom(string[] pool)
    {
        int index = rng.Next(pool.Length);
        SfxCmd.Play(pool[index], 0.9f);
    }
}