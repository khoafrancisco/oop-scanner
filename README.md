## Tổng quan

Chương trình console đơn giản dùng để quét HTTP: lấy danh sách đường dẫn từ `Assessment.Words`, mỗi từ được dùng làm thư mục (path) và yêu cầu file `flag.txt` tương ứng trên host mục tiêu.

---

## Cấu trúc chính (Luồng thực thi)

1. **Entry point**
    - `static async Task<int> Main(string[] args)`
    - Nhận `baseUrl` từ `args[0]`
    - Trả mã thoát:  
      - `0`: Thành công  
      - `2/3`: Lỗi đầu vào hoặc không load được wordlist

2. **Argument parsing & normalization**
    - Kiểm tra `args.Length`
    - Thêm `http://` nếu thiếu
    - Đảm bảo `baseUrl` kết thúc bằng `/`

3. **Tải wordlist**
    - Tạo instance: `new Words()`
    - Gọi: `wordsInstance.GetWordList()`
    - Kết quả lưu vào: `IEnumerable<string> wordList`  
      *(Lưu ý: `GetWordList()` là method instance, không phải static)*

4. **HTTP client**
    - Tạo một `HttpClient` dùng lại với `Timeout = 8s`
    - Sử dụng `using var client = ...` để giải phóng tài nguyên

5. **Vòng lặp quét**
    - Duyệt: `foreach (var word in wordList)`
    - Bỏ qua item rỗng, chuẩn hoá `word` bằng `Trim()` và `TrimStart('/')`
    - Tạo URL: `` $"{baseUrl}{word}/flag.txt" ``
    - Gửi GET tới URL
      - Nếu `IsSuccessStatusCode`: Đọc `res.Content` và in `FOUND` + nội dung
      - Nếu không phải 404: In mã trạng thái
    - Bắt `TaskCanceledException` (timeout) và Exception chung để log lỗi

6. **Kết thúc**
    - In `Scan complete.`
    - `return 0`

---

## Vài chi tiết kỹ thuật quan trọng

- **Tại sao dùng `IEnumerable<string>`?**  
  `GetWordList()` trả về `HashSet<string>`, nên dùng `IEnumerable<string>` làm kiểu chung thay vì `string[]`. Đếm phần tử dùng `wordList.Count()` (LINQ) thay vì `.Length`.

- **Tái sử dụng `HttpClient`**  
  Tốt — tránh tạo mới cho mỗi request.

- **Sử dụng `using var res = await client.GetAsync(url);`**  
  Đảm bảo `HttpResponseMessage` được dispose nhanh.

- **Xử lý lỗi**  
  Timeout và ngoại lệ chung được catch để chương trình tiếp tục quét các đường dẫn khác.
