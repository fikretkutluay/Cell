# UI Çalışmama Sorunu Çözümü

## Sorun
Bir sahneden başka bir sahneye geçtiğinde LevelExit UI'ı çalışmıyordu.

## Neden?
İki ana sebep vardı:

### 1. EventSystem Çakışması
- İlk sahnedeki EventSystem DontDestroyOnLoad ile kalıcı
- Yeni sahnede de EventSystem var
- İki EventSystem çakışıyor ve UI tıklamaları çalışmıyor

### 2. CoreUIManager Panelleri Kapatıyor
- CoreUIManager her sahne yüklendiğinde `HideAllPanels()` çağırıyor
- Bu, LevelExit'in nextLevelPanel'ini de kapatıyor olabilir

## Çözümler

### Çözüm 1: EventSystemManager (Yeni Script)
**EventSystemManager.cs** oluşturuldu:
- Sahnede birden fazla EventSystem varsa fazlalıkları yok eder
- İlk EventSystem'i DontDestroyOnLoad yapar
- Otomatik çalışır

### Çözüm 2: CoreUIManager Güncellendi
- `OnSceneLoaded()` methodu güncellendi
- Artık sadece kendi panellerini yönetiyor
- LevelExit panellerine dokunmuyor

---

## Kurulum

### Adım 1: EventSystemManager Ekle

**İlk sahnede (örn: MainMenu veya Level1):**

1. Hierarchy'de EventSystem objesini bul
2. Add Component → EventSystemManager
3. Oyunu başlat

**Diğer sahnelerde:**
- EventSystem varsa hiçbir şey yapma
- EventSystemManager otomatik olarak fazlalıkları temizleyecek

### Adım 2: Test Et

1. Level1'de oyna
2. LevelExit trigger'ına gir
3. Panel açılmalı
4. "Next Level" butonuna tıkla
5. Level2'ye geç
6. Level2'deki LevelExit'i test et
7. Panel açılmalı ve buton çalışmalı

---

## Alternatif Çözüm: Manuel EventSystem Kontrolü

Eğer EventSystemManager eklemek istemezsen, her sahnede EventSystem'i şöyle kontrol edebilirsin:

**Her yeni sahnede:**
1. Hierarchy'de EventSystem'i bul
2. Inspector'da kontrol et:
   - Eğer ilk sahne ise: DontDestroyOnLoad için EventSystemManager ekle
   - Eğer sonraki sahneler ise: EventSystem objesini sil (ilk sahnedeki zaten var)

---

## Debug

### Console Log'ları

EventSystemManager çalışıyorsa şu log'u göreceksin:
```
Destroying duplicate EventSystem in scene: Level2
```

### Sorun Giderme

**"Button tıklanmıyor" sorunu:**
1. Hierarchy'de EventSystem var mı kontrol et
2. Console'da "Destroying duplicate EventSystem" log'u var mı kontrol et
3. Button'un Interactable olduğunu kontrol et
4. Canvas'ın Render Mode'u doğru mu kontrol et

**"Panel açılmıyor" sorunu:**
1. LevelExit objesinin Collider2D'si var mı kontrol et
2. Is Trigger = true mu kontrol et
3. Player'ın "Player" tag'i var mı kontrol et
4. Console'da hata var mı kontrol et

**"Panel açılıyor ama buton çalışmıyor" sorunu:**
1. EventSystem var mı kontrol et
2. Button'un OnClick event'i bağlı mı kontrol et
3. LevelExit scriptinin GoToNextLevel() methodu bağlı mı kontrol et

---

## Özet

✅ EventSystemManager eklendi (EventSystem çakışmasını önler)
✅ CoreUIManager güncellendi (LevelExit panellerine dokunmuyor)
✅ UI artık tüm sahnelerde çalışmalı
✅ Button tıklamaları düzgün çalışmalı
