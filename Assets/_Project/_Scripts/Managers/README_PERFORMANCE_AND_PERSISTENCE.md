# Performance Optimization & Stat Persistence

## 1. Performans Optimizasyonu

### Sorun
GraphNode kullanan sahnelerde oyun aşırı kasıyordu.

### Neden?
- Her frame çok fazla Debug.Log çağrısı yapılıyordu
- Gereksiz log'lar CPU'yu meşgul ediyordu
- Özellikle çok enemy olduğunda her biri log yazıyordu

### Çözüm
**LeukocyteAI.cs** - Tüm gereksiz Debug.Log'lar kaldırıldı:
- State geçiş log'ları kaldırıldı
- Path bulma log'ları kaldırıldı
- Node'a ulaşma log'ları kaldırıldı
- Sadece kritik error log'ları kaldı

### Sonuç
- Oyun artık akıcı çalışmalı
- FPS düşüşü olmamalı
- Çok enemy olsa bile performans sorunsuz

---

## 2. Stat Persistence (Sahneler Arası Stat Koruma)

### Sorun
Karakter sahne değiştirdiğinde kazandığı statlar sıfırlanıyordu.

### Çözüm
**GameProgressManager.cs** - Yeni singleton manager eklendi:
- DontDestroyOnLoad ile sahneler arası kalıcı
- Player statlarını otomatik kaydeder ve yükler
- Sahne geçişlerinde statları korur

### Nasıl Çalışır?

**1. Otomatik Kayıt ve Yükleme:**
```csharp
// Sahne değiştiğinde otomatik çalışır
GameProgressManager.Instance.LoadNextScene("Level2");
```

**2. Manuel Kontrol (İsteğe Bağlı):**
```csharp
// Statları kaydet
GameProgressManager.Instance.SaveProgress();

// Statları yükle
GameProgressManager.Instance.LoadProgress();

// Statları sıfırla (oyun baştan başladığında)
GameProgressManager.Instance.ResetProgress();
```

---

## Kurulum

### Adım 1: GameProgressManager Oluştur

1. Hierarchy'de boş GameObject oluştur
2. İsmini "GameProgressManager" yap
3. GameProgressManager component'ini ekle
4. Inspector'da PlayerStats asset'ini ata

### Adım 2: LevelExit Kullan (Zaten Var!)

Mevcut LevelExit sistemi güncellendi:

**Otomatik çalışır:**
- Player LevelExit trigger'ına girdiğinde panel açılır
- "Next Level" butonuna tıklandığında statlar otomatik kaydedilir
- Bir sonraki sahneye geçer

**Hiçbir şey yapman gerekmez!** Mevcut LevelExit objeleri artık statları otomatik kaydediyor.

### Adım 3: Test Et

1. Level1'de oyna
2. Stat kazan (enemy öldür, upgrade al)
3. LevelExit'e dokun
4. Level2'de statların korunmuş olmalı

---

## Kullanım Örnekleri

### Örnek 1: Mevcut LevelExit Sistemi (Otomatik Çalışıyor!)

Mevcut LevelExit zaten güncellendi:
- Player trigger'a girdiğinde panel açılır
- "Next Level" butonuna tıklandığında statlar kaydedilir
- Otomatik sahne geçişi yapılır

**Hiçbir kod yazmana gerek yok!**

### Örnek 2: Boss Öldürünce Sahne Geçişi

```csharp
// BrainBoss.cs içinde
private void Die()
{
    IsDead = true;
    StopAllCoroutines();

    Debug.Log("Brain Boss defeated!");

    if (animator) animator.SetTrigger("Die");
    
    // Statları kaydet ve sonraki sahneye geç
    if (GameProgressManager.Instance != null)
    {
        GameProgressManager.Instance.LoadNextScene("VictoryScene");
    }

    Destroy(gameObject, 2f);
}
```

### Örnek 3: Oyun Baştan Başladığında

```csharp
// MainMenu.cs veya GameOver.cs içinde
public void RestartGame()
{
    if (GameProgressManager.Instance != null)
    {
        GameProgressManager.Instance.ResetProgress();
    }
    SceneManager.LoadScene("Level1");
}
```

### Örnek 4: Manuel Kayıt (Checkpoint)

```csharp
// Checkpoint.cs
private void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.CompareTag("Player"))
    {
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.SaveProgress();
            Debug.Log("Checkpoint saved!");
        }
    }
}
```

---

## Önemli Notlar

### PlayerStats ScriptableObject
- PlayerStats bir ScriptableObject olduğu için sahneler arası zaten paylaşılır
- Ama Unity Editor'da Play Mode'dan çıkınca değerler sıfırlanır
- GameProgressManager bu değerleri runtime'da tutar

### DontDestroyOnLoad
- GameProgressManager sahneler arası yok olmaz
- Sadece bir tane instance olmalı (singleton)
- Yeni sahne yüklendiğinde otomatik çalışır

### Scene Build Settings
- Tüm sahnelerin Build Settings'e eklenmiş olması gerekir
- File → Build Settings → Add Open Scenes

---

## Debug

### Console Log'ları

Başarılı kurulumda şu log'ları göreceksin:

```
Progress saved: HP=80/120, Damage=15, Speed=6
Progress loaded: HP=80/120, Damage=15, Speed=6
```

### Sorun Giderme

**"GameProgressManager not found!" hatası:**
- Hierarchy'de GameProgressManager objesi var mı kontrol et
- Component eklenmiş mi kontrol et

**"PlayerStats reference is null!" hatası:**
- GameProgressManager Inspector'ında PlayerStats asset'i atanmış mı kontrol et

**Statlar yüklenmiyor:**
- GameProgressManager'ın DontDestroyOnLoad çalışıyor mu kontrol et
- Console'da "Progress saved" ve "Progress loaded" log'larını kontrol et

---

## Özet

✅ Performans optimize edildi (Debug.Log'lar kaldırıldı)
✅ Stat persistence sistemi eklendi
✅ Sahneler arası geçiş otomatik çalışıyor
✅ LevelExit component'i hazır
✅ Manuel kontrol mümkün (SaveProgress, LoadProgress, ResetProgress)
