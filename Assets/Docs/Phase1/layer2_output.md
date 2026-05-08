# LAYER 2: TECHNICAL APPROACH — NextDay

> Created: 2026-05-08  
> Updated: 2026-05-08  
> Input: layer1_output.md  
> Status: Approved (Phương án B được chọn)

---

## Phân tích từ Lớp 1

- **Genre:** Survival Sandbox với Day/Night Cycle
- **Art Style:** 2.5D Isometric 3D Low-poly (camera góc nghiêng tạo độ sâu, art direction 2D)
- **Platform:** PC
- **Scope:** Solo indie
- **Timeline:** Rảnh thì làm, chưa có deadline
- **Gameplay:** Ngày (thu thập → xây dựng → nâng cấp) + Đêm (phòng thủ → chuẩn bị)
- **Engine:** Unity URP

---

## Các phương án đề xuất

### Phương án A — MonoBehaviour thuần + ScriptableObject Config

**Mô tả:** Kiến trúc đơn giản với MonoBehaviour làm logic chính, ScriptableObject chỉ dùng cho config (stats, item data). Phù hợp solo indie muốn bắt đầu nhanh.

**Stack:**
- Unity 2022 LTS URP
- Cinemachine (camera isometric)
- TextMeshPro (UI)
- Input System (input mới)

**Patterns:**
- Singleton cho GameManager (đơn giản cho solo)
- State Machine cho Day/Night cycle
- Object Pool cho enemy/projectile

**Asset Pipeline:**
- Blender → FBX 3D models (low-poly)
- Unity Sprite Atlas cho UI icons
- FMOD hoặc Unity Audio Source (tùy ngân sách)

**Ưu điểm:**
- Dễ bắt đầu, learning curve thấp
- Code trực quan, dễ debug
- Phù hợp solo indie timeline rảnh

**Nhược điểm:**
- Khó mở rộng khi game phức tạp
- Coupling cao giữa hệ thống
- Không tối ưu cho nhiều entity

**Rủi ro:**
- Nếu game mở rộng, cần refactor lớn sau này

**Độ khó:** Beginner  
**Ước tính thời gian:** 3-4 tháng cho vertical slice

---

### Phương án B — ScriptableObject Event-Driven (Lite)

**Mô tả:** Dùng ScriptableObject làm data layer + event channels để tách biệt logic và data. Hệ thống subscribe/unsubscribe events thay vì direct reference. Phù hợp survival sandbox với nhiều hệ thống tương tác.

**Stack:**
- Unity 2022 LTS URP
- Cinemachine
- TextMeshPro
- Input System
- DOTween (animation tween)

**Patterns:**
- Observer (ScriptableObject Events)
- State Machine (Day/Night cycle)
- Object Pool (enemy, projectile)
- Command (build/place action)

**Asset Pipeline:**
- Blender → FBX 3D models (low-poly)
- Unity Sprite Atlas cho UI icons
- FMOD hoặc Unity Audio Source

**Ưu điểm:**
- Tách biệt logic và data, dễ test
- Dễ mở rộng thêm hệ thống mới
- Coupling thấp, dễ refactor
- Phù hợp survival sandbox với nhiều hệ thống

**Nhược điểm:**
- Learning curve cao hơn một chút
- Cần quản lý event channels cẩn thận
- Debug event flow khó hơn direct call

**Rủi ro:**
- Nếu không quen với event-driven, có thể gặp khó khăn ban đầu

**Độ khó:** Intermediate  
**Ước tính thời gian:** 4-5 tháng cho vertical slice

---

## Đề xuất cho Solo Indie

Với **scope solo indie** + **timeline rảnh**, tôi đề xuất:

**Phương án A** nếu:
- Bạn mới bắt đầu với Unity
- Muốn vertical slice nhanh (3-4 tháng)
- Game không quá phức tạp

**Phương án B** nếu:
- Bạn đã quen với Unity
- Muốn kiến trúc có thể mở rộng lâu dài
- Game có nhiều hệ thống tương tác (thu thập, xây dựng, phòng thủ, inventory...)

---

## Chọn phương án

**Đã chọn: Phương án B — ScriptableObject Event-Driven (Lite)**

**Lý do:**
- Tách biệt logic và data giúp AI hiểu rõ responsibility từng module
- Event-driven làm coupling thấp, AI có thể implement module độc lập
- Dễ test từng module riêng lẻ
- Dễ mở rộng thêm hệ thống mới mà không refactor toàn bộ
- Phù hợp survival sandbox với nhiều hệ thống tương tác

---

## Phương án đã chọn (Chi tiết)

**Mô tả:** Dùng ScriptableObject làm data layer + event channels để tách biệt logic và data. Hệ thống subscribe/unsubscribe events thay vì direct reference.

**Stack:**
- Unity 2022 LTS URP
- Cinemachine
- TextMeshPro
- Input System
- DOTween (animation tween)
- Stable Diffusion 1.5 / SDXL (AI asset generation)
- ControlNet (composition/pose control)
- ESRGAN (AI upscaling)

**Patterns:**
- Observer (ScriptableObject Events)
- State Machine (Day/Night cycle)
- Object Pool (enemy, projectile)
- Command (build/place action)

**Asset Pipeline:**
- Blender → FBX 3D models (low-poly)
- Stable Diffusion 1.5 / SDXL → Concept art, textures, UI icons
- ControlNet → Composition/pose control cho concept art
- ESRGAN → Upscale textures lên high-res
- Unity Sprite Atlas cho UI icons
- FMOD hoặc Unity Audio Source

**Độ khó:** Intermediate
**Ước tính thời gian:** 4-5 tháng cho vertical slice

---

## AI Asset Generation Workflow

### Stable Diffusion 1.5 / SDXL trong pipeline

**Vai trò:** Tạo concept art, textures, UI icons nhanh chóng cho solo indie.

**Workflow:**

1. **Lớp 3 (Technical Plan)** → AI generate asset spec cho từng module
2. **SD 1.5/SDXL** → Generate concept art dựa trên spec:
   - Concept cho nhân vật, kẻ thù
   - Concept cho môi trường, căn cứ
   - Concept cho công trình, building
3. **Human review** → Chọn concept phù hợp, refine prompt
4. **SD 1.5/SDXL + ControlNet** → Generate textures từ concept:
   - Terrain textures (đất, đá, cỏ)
   - Building textures (gỗ, đá, kim loại)
   - Object textures (item, tool, weapon)
5. **ESRGAN** → Upscale textures lên high-res (2K/4K)
6. **Unity import** → Import textures, tweak nếu cần

**Ưu điểm cho solo indie:**
- Không cần skill vẽ chuyên sâu
- Tạo asset nhanh, tiết kiệm thời gian
- Có thể iterate concept nhanh chóng
- Phù hợp timeline rảnh (không deadline)

**Nhược điểm:**
- Cần GPU mạnh để chạy SD 1.5/SDXL
- Cần học prompt engineering
- Texture có thể không nhất quán (cần style tuning)
- Vẫn cần human review và refine

**Gợi ý prompt style cho NextDay:**
- Style: "low-poly 3D, isometric view, stylized, vibrant colors"
- Environment: "alien planet LSS-78, sci-fi wilderness, day/night cycle"
- Buildings: "survival base, defensive structures, modular design"
- UI: "clean icons, readable, sci-fi aesthetic"
