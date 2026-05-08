## 📦 LiteGlowUnity2D

**LiteGlowUnity2D** — модульная система 2D свечения для Unity URP, построенная на кастомном `ScriptableRendererFeature`.
Позволяет создавать гибкое GPU-освещение для SpriteRenderer с поддержкой масок, группового рендера и пост-композиции через шейдерный pipeline.

---

## 🎥 Video Tutorial

Installation and setup guide:

- Telegram — [Уголочек программиста](https://t.me/ugolokprogramistadayemayo/9)

---

### ✨ Основные возможности

* Многопроходный URP рендеринг (Shadow / Light / Mask / Composite)
* GPU-based 2D glow без посторонних камер
* Поддержка SpriteRenderer и групп объектов
* Маскирование света и альфа-обработка
* Настраиваемый glow (радиус, резкость, интенсивность)
* RTHandle pipeline (современный URP подход)
* Полностью shader-driven композиция

---

### 🧠 Архитектура

Система разделена на два основных слоя:

**1. Runtime компоненты**

* `LiteGlow2D` — источник света / glow компонент

**2. Rendering pipeline**

* `LiteGlow2DFeature` — URP RenderFeature

  * Shadow Pass
  * Light Pass
  * Mask Pass
  * Composite Pass

---

### 📁 Структура проекта

```
LiteGlowUnity2D
├── Auixliary
│   ├── Editor
│   │   ├── LiteGlow2DEditor.cs
│   │   └── Window
│   │       └── LiteGlow2DDebugWindow.cs
│   └── LiteGlow2D.cs
├── Feature
│   └── LiteGlow2DFeature.cs
└── Shader
    ├── Composite.shader
    └── LightMask.shader
```

---

### ⚙️ Pipeline концепция

```
LiteGlow2D (sources)
        ↓
Shadow Pass
        ↓
Light / Mask separation
        ↓
RT composition
        ↓
Composite shader → final camera image
```

---

### 🎯 Назначение

Система рассчитана на:

* 2D игры с динамическим освещением
* UI/World hybrid lighting
* Pixel / stylized lighting
* Custom rendering pipelines поверх URP

---

### 🚀 Getting Started

---

### 📥 Установка

Скопируйте папку `LiteGlowUnity2D` в ваш Unity проект:

```
Assets/LiteGlowUnity2D
```

Убедитесь, что проект использует **Universal Render Pipeline (URP)**.

---

### 🧩 Подключение Render Feature

1. Откройте ваш URP Renderer:

   ```
   Project Settings → Graphics → Scriptable Render Pipeline Settings
   ```

2. Найдите активный Renderer Data:

   ```
   UniversalRendererData (Forward Renderer)
   ```

3. Добавьте Feature:

   ```
   Add Renderer Feature → LiteGlow2DFeature
   ```

---

### 💡 Создание источника света

Добавьте компонент:

```csharp
LiteGlow2D
```

на любой `GameObject` со `SpriteRenderer`.

---

### ⚙️ Базовая настройка

Пример минимальной конфигурации:

* `intensity` — сила свечения
* `color` — цвет свечения

---

### 🎨 Режимы рендера

| Mode       | Описание                               |
| ---------- | -------------------------------------- |
| Current    | Использует текущий SpriteRenderer      |
| Sprite     | Рендер отдельного sprite               |
| ChildGroup | Все SpriteRenderer в дочерних объектах |

---

### 🧪 Быстрый пример

1. Создайте `Sprite` объект
2. Добавьте `SpriteRenderer`
3. Добавьте `LiteGlow2D`
4. Настройте:

```text
Intensity: 2
Color: Cyan
Mode: Current
```

---

### 🖥 Debug (опционально)

Используйте:

```
LiteGlow2DDebugWindow
```

для проверки активных источников свечения и буферов рендера.

---

### ⚠️ Требования

* Unity **2022.3+** (рекомендуется)
* Universal Render Pipeline (URP)
* Linear color space (рекомендуется)
* GPU с поддержкой RenderTextures

---

### 📌 Примечания

* Система работает в `ExecuteAlways` (редактор + runtime)
* Все источники автоматически регистрируются в `LiteGlow2D.Instances`
* RenderFeature управляет всей графикой через CommandBuffer pipeline

---

## [Assets/_Project/LiteGlowUnity2D/Auixliary](Assets/_Project/LiteGlowUnity2D/Auixliary)

## LiteGlow2D {#liteglow2d}

[./LiteGlow2D.cs](Assets/_Project/LiteGlowUnity2D/Auixliary/LiteGlow2D.cs)

`LiteGlow2D` — компонент 2D свечения для SpriteRenderer, предоставляющий гибкую систему настройки glow-эффекта (интенсивность, радиус, резкость, режимы маски и сглаживания), а также поддержку группового рендера и динамической регистрации экземпляров.

---

### 📦 Возможности

* Управление 2D glow-эффектом для спрайтов
* Поддержка разных режимов рендера:

  * Current (текущий SpriteRenderer)
  * Sprite (резервный режим под одиночный спрайт)
  * ChildGroup (все SpriteRenderer в иерархии)
* Глобальный список активных источников свечения (`Instances`)
* Настройка параметров свечения:

  * интенсивность
  * радиус
  * резкость
  * цвет
  * смещение и поворот
* Поддержка текстурной маски свечения
* Режимы обработки альфа-канала и масок
* Система сглаживания свечения (Smooth mode)
* Автоматическая проверка и нормализация параметров в Editor (`OnValidate`)
* Поддержка `ExecuteAlways` (работает в Edit Mode)

---

### Класс

```csharp
[ExecuteAlways]
public class LiteGlow2D : MonoBehaviour
```

---

### Поля

#### Instances

Глобальный список всех активных компонентов свечения.

```csharp
public static List<LiteGlow2D> Instances = new();
```

---

#### typeRender

Определяет режим получения `SpriteRenderer` для рендера свечения.

```csharp
public TypeRender typeRender;
```

---

#### intensity

Общая интенсивность свечения.

```csharp
public float intensity = 1f;
```

---

#### size

Размер области свечения.

```csharp
public Vector2 size = Vector2.one;
```

---

#### offset

Смещение центра свечения относительно объекта.

```csharp
public Vector2 offset = Vector2.zero;
```

---

#### angle

Угол поворота свечения.

```csharp
public float angle = 0f;
```

---

#### color

Цвет свечения.

```csharp
public Color color = Color.white;
```

---

#### mode

Режим обработки свечения.

```csharp
[SerializeField]
private ModeType mode;
```

---

#### useTexture

Использовать текстуру маски свечения.

```csharp
[SerializeField]
private bool useTexture;
```

---

#### glowRadius

Радиус распространения свечения.

```csharp
[SerializeField, Range(0.1f, 2.0f)]
private float glowRadius = 0.8f;
```

---

#### glowSharpness

Резкость края свечения.

```csharp
[SerializeField, Range(1.0f, 10.0f)]
private float glowSharpness = 4.0f;
```

---

#### smoothStrength

Сила сглаживания свечения.

```csharp
[SerializeField, Range(0.1f, 5f)]
private float smoothStrength = 1.5f;
```

---

#### smoothThreshold

Порог сглаживания.

```csharp
[SerializeField, Range(0.0f, 1.0f)]
private float smoothThreshold = 0.5f;
```

---

#### smoothPower

Кривая сглаживания.

```csharp
[SerializeField, Range(0.0f, 3.0f)]
private float smoothPower = 1.2f;
```

---

#### invertSmooth

Инверсия сглаживания.

```csharp
[SerializeField]
private bool invertSmooth = false;
```

---

#### alphaCutoff

Порог отсечения альфа-канала.

```csharp
[SerializeField, Range(0.0f, 1.0f)]
private float alphaCutoff = 0.5f;
```

---

#### brushTextureSprite

Спрайт текстуры кисти для свечения.

```csharp
[SerializeField]
private Sprite brushTextureSprite;
```

---

### Свойства

#### BrushTexture

Текстура кисти свечения.

```csharp
public Texture2D BrushTexture { get; }
```

---

#### AlphaCutoff

Порог альфа-отсечения.

```csharp
public float AlphaCutoff { get; }
```

---

#### Mode

Текущий режим свечения (int представление).

```csharp
public int Mode { get; }
```

---

#### GlowRadius

Радиус свечения с защитой от отрицательных значений.

```csharp
public float GlowRadius { get; set; }
```

---

#### GlowSharpness

Резкость свечения с ограничением минимального значения.

```csharp
public float GlowSharpness { get; set; }
```

---

#### UseTexture

Флаг использования текстуры.

```csharp
public bool UseTexture { get; set; }
```

---

#### SmoothStrength

Сила сглаживания.

```csharp
public float SmoothStrength { get; }
```

---

#### SmoothThreshold

Порог сглаживания.

```csharp
public float SmoothThreshold { get; }
```

---

#### SmoothPower

Кривая сглаживания.

```csharp
public float SmoothPower { get; }
```

---

#### InvertSmooth

Инверсия сглаживания.

```csharp
public bool InvertSmooth { get; }
```

---

#### \_spriteForRender

Внутренний спрайт для рендера.

```csharp
public Sprite _spriteForRender;
```

---

#### spriteRenderers

Список `SpriteRenderer` в зависимости от режима рендера.

```csharp
public SpriteRenderer[] spriteRenderers { get; }
```

---

### Методы

#### OnEnable()

Регистрирует компонент в глобальном списке `Instances`.

```csharp
private void OnEnable()
```

---

#### OnDisable()

Удаляет компонент из глобального списка `Instances`.

```csharp
private void OnDisable()
```

---

#### GetBounds()

Возвращает bounding box эффекта свечения с учетом offset.

```csharp
public Bounds GetBounds()
```

---

#### OnValidate()

Валидация и нормализация параметров в редакторе.

* ограничение минимальных значений
* проверка enum режима

```csharp
private void OnValidate()
```

---

## [Assets/_Project/LiteGlowUnity2D/Feature](Assets/_Project/LiteGlowUnity2D/Feature)

## LiteGlow2DFeature {#liteglow2dfeature}

[./LiteGlow2DFeature.cs](Assets/_Project/LiteGlowUnity2D/Feature/LiteGlow2DFeature.cs)

`LiteGlow2DFeature` — кастомный `ScriptableRendererFeature` для URP, реализующий многопроходную систему 2D-свечения. Основан на разделении света на shadow / light / mask буферы с последующим compositing в финальный цвет камеры.

---

### 📦 Возможности

* Полноценный URP RenderFeature для 2D glow системы
* Многопроходный рендеринг:

  * Shadow Pass (основа сцены свечения)
  * Light Pass (основные источники света)
  * Mask Pass (маски свечения)
  * Composite Pass (финальный сбор изображения)
* Использование RTHandle для рендер-таргетов
* Глобальные буферы света:

  * LightBuffer
  * MaskBuffer
* Поддержка кастомных материалов:

  * LightMask shader
  * Composite shader
* Сортировка источников света по типам рендера и режимам
* Поддержка Sprite-based и Group-based рендера
* Материальный pipeline через `MaterialPropertyBlock`
* GPU-friendly батчинг через CommandBuffer
* Очистка ресурсов через `Dispose`

---

### Класс

```csharp
public class LiteGlow2DFeature : ScriptableRendererFeature
```

---

### Settings

#### Settings

Настройки RenderFeature.

```csharp
[System.Serializable]
public class Settings
{
    public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
}
```

---

### SHARED (глобальные ресурсы)

#### shadowMapRT

RTHandle для теневого буфера свечения.

```csharp
public static RTHandle shadowMapRT;
```

---

#### lightMapRT

RTHandle для основного светового буфера.

```csharp
public static RTHandle lightMapRT;
```

---

#### maskMapRT

RTHandle для масок свечения.

```csharp
public static RTHandle maskMapRT;
```

---

#### LightScreen

Глобальный коэффициент интенсивности экрана света.

```csharp
public static float LightScreen = 0f;
```

---

#### LightScreenColor

Глобальный цвет света экрана.

```csharp
public static Color LightScreenColor = Color.white;
```

---

#### LightBuffer

Буфер всех активных источников света.

```csharp
private static List<LiteGlow2D> LightBuffer = new();
```

---

#### MaskBuffer

Буфер источников света в режиме Mask.

```csharp
private static List<LiteGlow2D> MaskBuffer = new();
```

---

### ShadowPass

ShadowPass — формирует базовую карту освещения.

#### Назначение

* Подготовка shadowMapRT
* Разделение источников на Light / Mask
* Отрисовка базового свечения

#### Основная логика

* Очистка буферов
* Сбор объектов из `LiteGlow2D.Instances`
* Сортировка по режиму (`Mask` или обычный свет)
* Отрисовка через `DrawNormalLight`

---

### MaskPass

MaskPass — рендер масок свечения.

#### Назначение

* Рендер объектов с режимом Mask
* Запись в maskMapRT

#### Особенности

* Использует MaskBuffer, заполненный в ShadowPass
* Отдельный RT для масок

---

### LightPass

LightPass — рендер основных источников света.

#### Назначение

* Рендер всех non-mask источников
* Запись в lightMapRT

#### Особенности

* Использует LightBuffer
* Отдельный буфер от shadow и mask

---

### CompositePass

CompositePass — финальная сборка изображения.

#### Назначение

* Комбинирование:

  * shadowMapRT
  * lightMapRT
  * maskMapRT
* Применение пост-эффектного шейдера

#### Материалы

```csharp
CoreUtils.CreateEngineMaterial("Hidden/LiteGlow2D/Composite");
```

#### Передаваемые параметры

* `_MainTex` → shadowMapRT
* `_LightTex` → lightMapRT
* `_MaskTex` → maskMapRT
* `_Light` → LightScreen
* `_LightColor` → LightScreenColor

---

### Жизненный цикл

#### Create()

Инициализация всех render pass.

```csharp
public override void Create()
```

---

#### AddRenderPasses()

Добавление всех passes в renderer pipeline.

```csharp
public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
```

---

#### Dispose(bool)

Освобождение RT ресурсов.

```csharp
protected override void Dispose(bool disposing)
```

---

### Helper методы

---

#### DrawSprite()

Рендер спрайта через Mesh + CommandBuffer.

```csharp
static void DrawSprite(CommandBuffer cmd, Sprite s, Material m, MaterialPropertyBlock propBlock, Matrix4x4 matrix)
```

---

#### CreateMesh()

Создаёт Mesh из Sprite (vertices, triangles, uv).

```csharp
static Mesh CreateMesh(Sprite sprite)
```

---

#### DrawNormalLight()

Основной метод рендера одного источника света.

##### Поддерживает:

* Sprite mode
* ChildGroup mode
* MaterialPropertyBlock параметры:

  * `_Intensity`
  * `_Mode`
  * `_UseTexture`
  * `_GlowRadius`
  * `_GlowSharpness`
  * `_Color`
  * `_AlphaCutoff`

##### Логика:

* Настройка shader properties
* Выбор режима рендера
* Построение matrix трансформации
* Отрисовка через CommandBuffer

---

### 🔧 Архитектурные особенности

* Разделение света на 3 независимых буфера (shadow/light/mask)
* Полностью CPU-driven сбор сцены света
* Батчинг через CommandBuffer (без GameObject draw calls)
* RTHandle система для URP compatibility
* Shader-driven финальный composite stage
* Поддержка Sprite sorting (layer + order + hierarchy)

---
