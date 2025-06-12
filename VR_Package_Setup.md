# Unity VR Package Kurulum Rehberi

## XR Hands Paketini Kurma Adımları:

### 1. Unity Editor'ı Açın
- Unity Hub'dan projeyi açın

### 2. Package Manager'ı Açın  
- Window > Package Manager

### 3. XR Hands Paketini Bulun
- Dropdown'dan "Unity Registry" seçin
- Arama kutusuna "XR Hands" yazın
- "XR Hands" paketini bulun

### 4. Paketi Kurun
- "XR Hands" paketine tıklayın
- "Install" butonuna basın

### 5. XR Interaction Toolkit'i Güncelleyin
- "XR Interaction Toolkit" paketini bulun
- En son versiyona güncelleyin

### 6. XR Plugin Management Ayarları
- Edit > Project Settings > XR Plug-in Management
- Meta/Oculus provider'ı etkinleştirin

## Alternatif Kurulum (manifest.json):

Assets/manifest.json dosyasına şu satırları ekleyin:

```json
{
  "dependencies": {
    "com.unity.xr.hands": "1.3.0",
    "com.unity.xr.interaction.toolkit": "2.5.2",
    "com.unity.xr.management": "4.4.0",
    "com.unity.xr.oculus": "4.1.2"
  }
}
```

## Hata Çözümü:
- Unity'yi yeniden başlatın
- Packages/manifest.json'u kontrol edin
- Console'daki hataları takip edin
