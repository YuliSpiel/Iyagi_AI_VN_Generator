using UnityEngine;
using UnityEditor;
using IyagiAI.Runtime;
using System.Collections.Generic;

namespace IyagiAI.Editor
{
    /// <summary>
    /// 샘플 엔딩 씬 생성 헬퍼
    /// </summary>
    public static class CreateSampleEndingScenes
    {
        [MenuItem("Iyagi/Create Sample Ending Scenes")]
        public static void CreateSamples()
        {
            // EndingSceneDatabase 생성
            string dbPath = "Assets/Resources/VNProjects/EndingSceneDatabase.asset";
            EndingSceneDatabase database = AssetDatabase.LoadAssetAtPath<EndingSceneDatabase>(dbPath);

            if (database == null)
            {
                database = ScriptableObject.CreateInstance<EndingSceneDatabase>();
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dbPath));
                AssetDatabase.CreateAsset(database, dbPath);
                Debug.Log($"Created EndingSceneDatabase at {dbPath}");
            }

            // 1. True Ending
            var trueEnding = CreateTrueEnding();
            database.endingScenes.Add(trueEnding);

            // 2. Value Ending (용기)
            var valueEnding = CreateValueEnding();
            database.endingScenes.Add(valueEnding);

            // 3. Normal Ending
            var normalEnding = CreateNormalEnding();
            database.endingScenes.Add(normalEnding);

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();

            Debug.Log($"✅ Created {database.endingScenes.Count} sample ending scenes!");
            EditorGUIUtility.PingObject(database);
        }

        private static EndingSceneData CreateTrueEnding()
        {
            var ending = ScriptableObject.CreateInstance<EndingSceneData>();
            ending.endingType = EndingType.TrueEnding;
            ending.endingTitle = "True Ending";
            ending.endingDescription = "You have achieved perfect balance and reached the ultimate ending.";

            ending.dialogueLines = new List<EndingDialogueLine>
            {
                new EndingDialogueLine
                {
                    speakerName = "Narrator",
                    dialogueText = "The journey has come to an end. Through countless choices, you found the path of true harmony.",
                    bgName = "sunset_sky",
                    bgmName = "emotional_theme"
                },
                new EndingDialogueLine
                {
                    speakerName = "Narrator",
                    dialogueText = "Your values shaped not just your destiny, but the world around you.",
                },
                new EndingDialogueLine
                {
                    speakerName = "Narrator",
                    dialogueText = "This is the true ending—a culmination of wisdom, courage, and compassion.",
                },
                new EndingDialogueLine
                {
                    speakerName = "Narrator",
                    dialogueText = "Thank you for playing.",
                    cgId = "ending_true"
                }
            };

            string assetPath = "Assets/Resources/VNProjects/Endings/TrueEnding.asset";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(assetPath));
            AssetDatabase.CreateAsset(ending, assetPath);

            return ending;
        }

        private static EndingSceneData CreateValueEnding()
        {
            var ending = ScriptableObject.CreateInstance<EndingSceneData>();
            ending.endingType = EndingType.ValueEnding;
            ending.endingTitle = "Value Ending";
            ending.endingDescription = "Your journey ends with your core values guiding the way.";

            ending.dialogueLines = new List<EndingDialogueLine>
            {
                new EndingDialogueLine
                {
                    speakerName = "Narrator",
                    dialogueText = "The path you chose reflected your deepest convictions.",
                    bgName = "peaceful_meadow",
                    bgmName = "calm_theme"
                },
                new EndingDialogueLine
                {
                    speakerName = "Narrator",
                    dialogueText = "Though not perfect, you stayed true to what you believed in.",
                },
                new EndingDialogueLine
                {
                    speakerName = "Narrator",
                    dialogueText = "Your legacy will be remembered by those you touched.",
                },
                new EndingDialogueLine
                {
                    speakerName = "Narrator",
                    dialogueText = "This is your ending.",
                    cgId = "ending_value"
                }
            };

            string assetPath = "Assets/Resources/VNProjects/Endings/ValueEnding.asset";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(assetPath));
            AssetDatabase.CreateAsset(ending, assetPath);

            return ending;
        }

        private static EndingSceneData CreateNormalEnding()
        {
            var ending = ScriptableObject.CreateInstance<EndingSceneData>();
            ending.endingType = EndingType.NormalEnding;
            ending.endingTitle = "Normal Ending";
            ending.endingDescription = "Your journey ends, though your path remains unclear.";

            ending.dialogueLines = new List<EndingDialogueLine>
            {
                new EndingDialogueLine
                {
                    speakerName = "Narrator",
                    dialogueText = "The story reaches its conclusion, but questions remain.",
                    bgName = "cloudy_sky",
                    bgmName = "melancholic_theme"
                },
                new EndingDialogueLine
                {
                    speakerName = "Narrator",
                    dialogueText = "Perhaps another path would have led to greater understanding.",
                },
                new EndingDialogueLine
                {
                    speakerName = "Narrator",
                    dialogueText = "But this is how it ends for now.",
                },
                new EndingDialogueLine
                {
                    speakerName = "Narrator",
                    dialogueText = "Will you try again?",
                    cgId = "ending_normal"
                }
            };

            string assetPath = "Assets/Resources/VNProjects/Endings/NormalEnding.asset";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(assetPath));
            AssetDatabase.CreateAsset(ending, assetPath);

            return ending;
        }
    }
}
