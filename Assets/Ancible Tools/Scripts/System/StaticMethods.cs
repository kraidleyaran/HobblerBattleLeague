using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Maze;
using Assets.Resources.Ancible_Tools.Scripts.System.SaveData;
using Assets.Resources.Ancible_Tools.Scripts.System.Skills;
using MessageBusLib;
using ProceduralToolkit;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public static class StaticMethods
    {
        public static List<T> QueryList<T>(this List<T> list, Predicate<T> query)
        {
            var returnList = new List<T>();
            for (var i = 0; i < list.Count; i++)
            {
                if (query.Invoke(list[i]))
                {
                    returnList.Add(list[i]);
                }
            }
            return returnList;
        }

        public static NullValue<T> QuerySingle<T>(this List<T> list, Predicate<T> query)
        {
            var returnValue = new NullValue<T>();
            for (var i = 0; i < list.Count; i++)
            {
                if (query.Invoke(list[i]))
                {
                    returnValue.SetValue(list[i]);
                    break;
                }
            }
            return returnValue;

        }

        public static void LerpMove(this Rigidbody2D rigidbody, float moveSpeed, float interpolation, Vector2 direction)
        {
            var position = rigidbody.position;
            position += Vector2.ClampMagnitude(moveSpeed * direction, moveSpeed);
            position = Vector2.Lerp(rigidbody.position, position, interpolation);
            //position.x = (float)Math.Round(position.x, DataController.ROUND_DECIMAL_PLACES);
            //position.y = (float)Math.Round(position.y, DataController.ROUND_DECIMAL_PLACES);
            rigidbody.position = position;
        }

        public static IEnumerator WaitForFrames(int frames, Action doAfter)
        {
            var frameCount = Time.frameCount + frames;
            yield return new WaitUntil(() =>
            {
                var currentFrameCount = Time.frameCount;
                return currentFrameCount >= frameCount;
            });
            doAfter.Invoke();
        }

        public static Vector2 ToVector2(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.y);
        }

        public static HitboxController SetupHitbox(this GameObject gameObject, Hitbox.Hitbox hitbox, CollisionLayer layer)
        {   
            var hitboxFilter = HitboxController.GenerateHitboxFilter(hitbox, layer);
            var hitboxCheckmsg = MessageFactory.GenerateHitboxCheckMsg();
            HitboxController controller = null;
            hitboxCheckmsg.DoAfter = hitboxController =>
            {
                controller = hitboxController;
            };
            var parentObj = gameObject;
            if (gameObject.transform.parent)
            {
                parentObj = gameObject.transform.parent.gameObject;
            }
            gameObject.SendMessageWithFilterTo(hitboxCheckmsg, parentObj, hitboxFilter);
            MessageFactory.CacheMessage(hitboxCheckmsg);
            if (!controller)
            {
                controller = Object.Instantiate(hitbox.Controller, parentObj.transform);
                controller.Setup(hitbox, layer);
            }
            return controller;
        }

        //public static Vector2 ToPixelPosition(this Vector2 vector)
        //{
        //    var returnValue = vector;

        //    var xMultiplier = vector.x / DataController.Interpolation;
        //    var trueXMulti = xMultiplier;
        //    var intX = 0;
        //    if (xMultiplier < 0f)
        //    {
        //        trueXMulti *= -1;
                
        //    }
        //    intX = Mathf.RoundToInt(trueXMulti);

        //    if (xMultiplier < 0f)
        //    {
        //        intX *= -1;
        //    }

        //    returnValue.x = intX * DataController.Interpolation;

        //    var yMultiplier = vector.y / DataController.Interpolation;
        //    var trueYMulti = yMultiplier;
        //    var intY = 0;
        //    if (yMultiplier < 0f)
        //    {
        //        trueYMulti *= -1;

        //    }
        //    intY = Mathf.RoundToInt(trueYMulti);

        //    if (yMultiplier < 0f)
        //    {
        //        intY *= -1;
        //    }

        //    returnValue.y = intY * DataController.Interpolation;

        //    return returnValue;
        //}

        public static Vector2 ToStaticDirections(this Vector2 vector)
        {
            var returnVector = vector;
            if (returnVector.x > 0)
            {
                returnVector.x = 1;
            }
            else if (returnVector.x < 0)
            {
                returnVector.x = -1;
            }

            if (returnVector.y > 0)
            {
                returnVector.y = 1;
            }
            else if (returnVector.y < 0)
            {
                returnVector.y = -1;
            }

            return returnVector;
        }

        public static Vector2Int ToVector2Int(this Vector2 vector, bool rounding = false)
        {
            if (rounding)
            {
                return new Vector2Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
            }
            return new Vector2Int((int)vector.x, (int)vector.y);

        }

        public static float GetHeightOfText(this Text uiObject, string text)
        {
            //var generator = new TextGenerator();
            if (string.IsNullOrEmpty(text))
            {
                return 0f;
            }
            else
            {
                var generator = new TextGenerator();
                var settings = uiObject.GetGenerationSettings(uiObject.rectTransform.rect.size);
                settings.verticalOverflow = VerticalWrapMode.Overflow;
                settings.fontSize = 0;
                settings.font = uiObject.font;
                settings.lineSpacing = uiObject.lineSpacing;
                settings.fontStyle = FontStyle.Normal;
                generator.Populate(text, settings);
                return (generator.lines.Count) * (uiObject.fontSize * settings.lineSpacing);
            }


            //return generator.GetPreferredHeight(text, settings) /*/ (settings.font.fontSize / (float)settings.fontSize)*/;
        }

        public static float GetWidthText(this Text uiObject, string text)
        {
            var generator = new TextGenerator();
            var settings = uiObject.GetGenerationSettings(uiObject.rectTransform.rect.size);
            settings.horizontalOverflow = uiObject.horizontalOverflow;
            settings.fontSize = uiObject.fontSize;
            return generator.GetPreferredWidth(text, settings) / (settings.font.fontSize / (float)settings.fontSize);
        }

        public static string[] GetFormmatedTextLines(this Text uiObject, string text)
        {
            var generator = new TextGenerator();
            var settings = uiObject.GetGenerationSettings(uiObject.rectTransform.rect.size);
            settings.verticalOverflow = VerticalWrapMode.Overflow;
            generator.Populate(text, settings);
            var lines = generator.lines;
            var formmatedLines = new List<string>();
            for (var i = 0; i < lines.Count; i++)
            {
                var startPosition = lines[i].startCharIdx;
                var endPosition = text.Length;
                if (i < lines.Count - 1)
                {
                    endPosition = lines[i + 1].startCharIdx;
                }
                var line = text.Substring(startPosition, endPosition - startPosition);
                line = line.TrimEnd(' ');
                line = line.Replace(Environment.NewLine, string.Empty);
                formmatedLines.Add(line);
            }
            formmatedLines.RemoveAll(string.IsNullOrEmpty);

            return formmatedLines.ToArray();
        }

        public static string ToSingleString(this string[] text)
        {
            var returnValue = string.Empty;
            for (var i = 0; i < text.Length; i++)
            {
                returnValue = i == 0 ? text[i] : $"{returnValue}{Environment.NewLine}{text[i]}";
            }
            return returnValue;
        }

        public static int GetLineCount(string text, int charsPerLine)
        {
            var textValue = text;
            var splitNewLines = textValue.Split('\n');
            var lineCount = splitNewLines.Length;
            for (var i = 0; i < splitNewLines.Length; i++)
            {
                var line = splitNewLines[i];
                if (line.Length > charsPerLine)
                {
                    var additionalLines = line.Length / charsPerLine;
                    var lineCheck = (additionalLines + 1) * charsPerLine;
                    if (lineCheck < line.Length)
                    {
                        additionalLines++;
                    }

                    lineCount += additionalLines;
                }
            }

            return lineCount;
        }

        public static Vector2 GetMouseQuadrant(Vector2 mousePos)
        {
            var middleScreen = new Vector2(Screen.width / 2f, Screen.height / 2f);
            var quadrant = Vector2.zero;
            if (mousePos.x > middleScreen.x)
            {
                quadrant.x = 1;
            }

            if (mousePos.y > middleScreen.y)
            {
                quadrant.y = 1;
            }

            return quadrant;
        }

        public static Vector2 ToNakedValues(this Vector2 vector)
        {
            var returnVector = vector;
            if (returnVector.x < 0)
            {
                returnVector.x *= -1;
            }

            if (returnVector.y < 0)
            {
                returnVector.y *= -1;
            }

            return returnVector;
        }

        public static Vector2Int ToCardinal(this Vector2 vector)
        {
            var naked = vector.ToNakedValues();
            var returnValue = Vector2Int.zero;
            if (naked.x > naked.y)
            {
                returnValue.x = vector.x > 0 ? 1 : -1;
            }
            else if (naked.y > naked.x)
            {
                returnValue.y = vector.y > 0 ? 1 : -1;
            }

            return returnValue;
        }

        public static Vector2Int Normalize(this Vector2Int vector)
        {
            var direction = Vector2Int.zero;
            if (vector.y > 0)
            {
                direction.y = 1;
            }
            else if (vector.y < 0)
            {
                direction.y = -1;
            }

            if (vector.x > 0)
            {
                direction.x = 1;
            }
            else if (vector.x < 0)
            {
                direction.x = -1;
            }

            return direction;
        }

        public static int DistanceTo(this Vector2Int origin, Vector2Int end)
        {
            var xValues = Mathf.Pow(end.x - origin.x, 2);
            var yValues = Mathf.Pow(end.y - origin.y, 2);
            return Mathf.RoundToInt(Mathf.Sqrt(xValues + yValues));
        }

        public static int Roll(this IntNumberRange range)
        {
            return Random.Range(range.Minimum, range.Maximum + 1);
        }

        public static float Roll(this FloatNumberRange range)
        {
            return Random.Range(range.Minimum, range.Maximum);
        }

        public static bool CoinFlip()
        {
            return Random.Range(0f, 1f) >= .5f;
        }

        public static string ApplyColorToText(string text, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
        }

        public static float ToZRotation(this Vector2 direction)
        {
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }

        public static Transform SetTransformPosition(this Transform transform, Vector2 position)
        {
            var pos = transform.position;
            pos.x = position.x;
            pos.y = position.y;
            transform.position = pos;
            return transform;
        }

        public static Transform SetLocalPosition(this Transform transform, Vector2 position)
        {
            var pos = transform.localPosition;
            pos.x = position.x;
            pos.y = position.y;
            transform.localPosition = pos;
            return transform;
        }

        public static string[] GetTraitDescriptions(this Trait[] traits)
        {
            return traits.Select(t => t.GetDescription()).Where(t => !string.IsNullOrEmpty(t)).ToArray();
        }

        public static Vector3 ToVector3(this Vector2 vector)
        {
            return new Vector3(vector.x, vector.y);
        }

        public static Vector3 ToVector3(this Vector2 vector, float z)
        {
            
            return new Vector3(vector.x, vector.y,z);
        }

        public static Transform SetLocalScaling(this Transform transform, Vector2 scale)
        {
            var localScale = transform.localScale;
            localScale.x = scale.x;
            localScale.y = scale.y;
            transform.localScale = localScale;
            return transform;
        }

        public static Transform SetLocalRotation(this Transform transform, float rotation)
        {
            var euler = transform.localRotation.eulerAngles;
            euler.z = rotation;
            transform.localRotation = Quaternion.Euler(euler);
            return transform;
        }

        public static string ReplaceSpacesWithUnderscores(this string text)
        {
            return text.Replace(" ", "_");
        }

        public static Vector2 ToPixelPerfect(this Vector2 vector)
        {
            var intVector = ((vector /*+ DataController.TrueZero*/) / DataController.Interpolation).ToVector2Int(true);
            var returnVector = new Vector2(intVector.x * DataController.Interpolation, intVector.y * DataController.Interpolation);
            //var diff = vector - returnVector;
            //if (diff.x < -DataController.TrueZero.x / 2f)
            //{
            //    returnVector.x -= DataController.Interpolation;
            //}
            //else if (diff.x > DataController.TrueZero.x / 2f)
            //{
            //    returnVector.x += DataController.Interpolation;
            //}

            //if (diff.y < -DataController.TrueZero.y / 2f)
            //{
            //    returnVector.y -= DataController.Interpolation;
            //}
            //else if (diff.y > DataController.TrueZero.y / 2f)
            //{
            //    returnVector.y += DataController.Interpolation;
            //}

            return returnVector;
        }

        public static int CalculateHappiness(this WellbeingStats stats, IntNumberRange caps)
        {
            var happiness = 0f;
            happiness += stats.Hunger * WellBeingController.HappinessPerHunger * -1;
            happiness += stats.Boredom * WellBeingController.HappinessPerBoredom * -1;
            happiness += stats.Fatigue * WellBeingController.HappinessPerFatigue * -1;
            happiness += stats.Ignorance * WellBeingController.HappinessPerIgnorance * -1;
            happiness = Mathf.Max(caps.Minimum, happiness);
            happiness = Mathf.Min(caps.Maximum, happiness);
            return Mathf.RoundToInt(happiness);
        }

        public static bool HasMoreThanOneDirection(this Directions directions)
        {
            var count = 0;
            if (directions.HasFlag(Directions.Up))
            {
                count++;
            }

            if (directions.HasFlag(Directions.Down))
            {
                count++;
            }

            if (directions.HasFlag(Directions.Left))
            {
                count++;
            }

            if (directions.HasFlag(Directions.Right))
            {
                count++;
            }

            return count > 0;
        }

        public static int DirectionCount(this Directions directions)
        {
            var count = 0;
            if (directions.HasFlag(Directions.Up))
            {
                count++;
            }

            if (directions.HasFlag(Directions.Down))
            {
                count++;
            }

            if (directions.HasFlag(Directions.Left))
            {
                count++;
            }

            if (directions.HasFlag(Directions.Right))
            {
                count++;
            }

            return count;
        }

        public static DoorRotation ToDoorRotation(this Directions direction)
        {
            switch (direction)
            {
                case Directions.Left:
                case Directions.Right:
                    return DoorRotation.Horizontal;
                case Directions.Down:
                case Directions.Up:
                    return DoorRotation.Vertical;
                default:
                    return DoorRotation.Vertical;
            }
        }

        public static Transform FlipX(this Transform transform, bool flip)
        {
            if (flip && transform.localScale.x > 0 || !flip && transform.localScale.x < 0)
            {
                var scale = transform.localScale;
                scale.x *= -1;
                transform.localScale = scale;
            }

            return transform;
        }

        public static Transform FlipY(this Transform transform, bool flip)
        {
            if (flip && transform.localScale.x > 0 || !flip && transform.localScale.x < 0)
            {
                var scale = transform.localScale;
                scale.x *= -1;
                transform.localScale = scale;
            }

            return transform;
        }

        public static int CalculateNextLevel(int currentLevel, int baseExperience, float experienceRate)
        {
            return (int) (baseExperience + baseExperience * currentLevel * experienceRate);
        }

        public static WellbeingStatType[] GetStatus(this WellbeingStats stats)
        {
            var status = new List<WellbeingStatType>();
            if (stats.Hunger > 0)
            {
                status.Add(WellbeingStatType.Hunger);
            }

            if (stats.Boredom > 0)
            {
                status.Add(WellbeingStatType.Boredom);
            }

            if (stats.Fatigue > 0)
            {
                status.Add(WellbeingStatType.Fatigue);
            }

            if (stats.Ignorance > 0)
            {
                status.Add(WellbeingStatType.Ignorance);
            }

            return status.ToArray();
        }

        public static float ToPixelMagnitude(this Vector2 vector)
        {
            var magnitude = vector.magnitude;
            var pixels = (int)(magnitude / DataController.Interpolation);
            var pixelMag = pixels * DataController.Interpolation;
            if (pixelMag < magnitude)
            {
                pixelMag += DataController.Interpolation;
            }

            return pixelMag;
        }

        public static int ToPixels(this float units)
        {
            return (int)(units / DataController.Interpolation);
        }

        public static Vector2 GetScreenQuadrant(Vector2 screenPos)
        {
            var quadrant = Vector2.zero;
            if (screenPos.x >= Screen.width / 2f)
            {
                quadrant.x = 1;
            }
            else
            {
                quadrant.x = -1;
            }

            if (screenPos.y >= Screen.height / 2f)
            {
                quadrant.y = 1;
            }
            else
            {
                quadrant.y = -1;
            }

            return quadrant;
        }

        public static T GetRandom<T>(this T[] array)
        {
            return array.Length > 1 ? array[Random.Range(0, array.Length)] : array[0];
        }

        public static T GetRandom<T>(this List<T> list)
        {
            return list.Count > 1 ? list[Random.Range(0, list.Count)] : list[0];
        }

        public static bool EqualityCompare(this float value, ComparisonType type, float otherValue)
        {
            switch (type)
            {
                case ComparisonType.Equal:
                    return value == otherValue;
                case ComparisonType.NotEqual:
                    return value != otherValue;
                case ComparisonType.GreaterThan:
                    return value > otherValue;
                case ComparisonType.GreaterThanOrEqual:
                    return value >= otherValue;
                case ComparisonType.LessThan:
                    return value < otherValue;
                case ComparisonType.LessThanOrEqual:
                    return value <= otherValue;
                default:
                    return true;
            }
        }

        public static bool EqualityCompare(this int value, ComparisonType type, int otherValue)
        {
            switch (type)
            {
                case ComparisonType.Equal:
                    return value == otherValue;
                case ComparisonType.NotEqual:
                    return value != otherValue;
                case ComparisonType.GreaterThan:
                    return value > otherValue;
                case ComparisonType.GreaterThanOrEqual:
                    return value >= otherValue;
                case ComparisonType.LessThan:
                    return value < otherValue;
                case ComparisonType.LessThanOrEqual:
                    return value <= otherValue;
                default:
                    return true;
            }
        }

        public static string ToDescription(this ComparisonType type)
        {
            switch (type)
            {
                case ComparisonType.Equal:
                    return "=";
                case ComparisonType.NotEqual:
                    return "!=";
                case ComparisonType.GreaterThan:
                    return ">";
                case ComparisonType.GreaterThanOrEqual:
                    return ">=";
                case ComparisonType.LessThan:
                    return "<";
                case ComparisonType.LessThanOrEqual:
                    return "<=";
            }

            return type.ToString();
        }

        public static string GetResultsDescription(this BattleUnitData data)
        {
            var description = string.Empty;
            if (data.RoundsPlayed > 0)
            {
                var damagePerRound = (float)data.TotalDamageDone / data.RoundsPlayed;
                description = $"Damage Done: {data.TotalDamageDone} - {damagePerRound:F1} ADPR";
                description = $"{description}{Environment.NewLine}Damage Taken: {data.TotalDamageTaken}";
                if (data.TotalHeals > 0)
                {
                    var healsPerRound = (float)data.TotalHeals / data.RoundsPlayed;
                    description = $"{description}{Environment.NewLine}Healing Done: {data.TotalHeals} - {healsPerRound:F1} AHPR";
                }
                description = $"{description}{Environment.NewLine}Rounds Active: {data.RoundsPlayed}";
                description = $"{description}{Environment.NewLine}Deaths: {data.Deaths}";
            }
            else
            {
                description = "Never Played";
            }

            return description;
        }

        public static int CalculateExperienceForLevel(this WorldSkill skill, int level)
        {
            return level > 1 ? Mathf.RoundToInt(skill.LevelExperience + skill.LevelExperience * (skill.ExperienceMultiplier * level)) : skill.LevelExperience;
        }

        public static int GetAverage(this int[] numbers)
        {
            var total = 0;
            for (var i = 0; i < numbers.Length; i++)
            {
                total += numbers[i];
            }

            return total / numbers.Length;
        }

        public static AbilityData ToData(this KeyValuePair<int, WorldAbility> ability)
        {
            return new AbilityData {Name = ability.Value.name, Priority = ability.Key};
        }

        public static Vector2IntData ToData(this Vector2Int vector)
        {
            return new Vector2IntData {X = vector.x, Y = vector.y};
        }

        

        public static SkillData ToData(this KeyValuePair<int, SkillInstance> skill)
        {
            return new SkillData
            {
                Skill = skill.Value.Instance.name,
                Experience = skill.Value.Instance.LevelExperience,
                Level = skill.Value.Level,
                Priority = skill.Key,
                Permanent = skill.Value.Permanent
            };
        }

        public static string ToFloatingText(this StatusEffectType type)
        {
            switch (type)
            {
                case StatusEffectType.Stun:
                    return "Stunned!";
                case StatusEffectType.Silence:
                    return "Silenced!";
                case StatusEffectType.Root:
                    return "Rooted!";
                case StatusEffectType.Mute:
                    return "Muted!";
                case StatusEffectType.Disarm:
                    return "Disarmed!";
                default:
                    return string.Empty;
            }
        }

        public static Color ToRarityColor(this ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common:
                    return ColorFactoryController.CommonItemRarity;
                case ItemRarity.Uncommon:
                    return ColorFactoryController.UnCommonItemRarity;
                case ItemRarity.Rare:
                    return ColorFactoryController.RareItemRarity;
                case ItemRarity.Epic:
                    return ColorFactoryController.EpicItemRarity;
                case ItemRarity.Legendary:
                    return ColorFactoryController.LegendaryItemRarity;
                case ItemRarity.Ancient:
                    return ColorFactoryController.AncientItemRarity;
                default:
                    return Color.white;
            }
        }

        public static void SetObjectLayer(this ParticleSystem system, int layer)
        {
            var systemCount = system.subEmitters.subEmittersCount;
            for (var i = 0; i < systemCount; i++)
            {
                var subSystem = system.subEmitters.GetSubEmitterSystem(i);
                subSystem.SetObjectLayer(layer);
            }
            system.gameObject.layer = layer;

        }

        public static string DoubleNewLine()
        {
            return $"{Environment.NewLine}{Environment.NewLine}";
        }

    }



    public struct NullValue<T>
    {
        public T Value;
        public bool HasValue;

        public NullValue(T value)
        {
            Value = value;
            HasValue = true;
        }

        public void SetValue(T value)
        {
            Value = value;
            HasValue = true;
        }

        
    }
}