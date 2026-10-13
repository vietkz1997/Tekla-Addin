;;;=============================================================================
;;; PROGRAM: Change-shape.lsp
;;; COMMAND: CS / CSH / ChangeShape
;;; DESCRIPTION: AutoLISP bridge cho công cụ Change-shape (Rebar Shape Studio).
;;;              Khởi động hoặc kích hoạt ứng dụng Change-shape.exe, hỗ trợ truyền
;;;              đối tượng MText thanh thép đang chọn vào tool.
;;; AUTHOR: BIM / Antigravity
;;;=============================================================================

(vl-load-com)

(defun c:CS () (c:ChangeShape))
(defun c:CSH () (c:ChangeShape))

(defun c:ChangeShape ( / appPath bridgePath ss ent elist text handle f )
  (princ "\n--- CHANGE-SHAPE PRO | REBAR SHAPE STUDIO (SIDD-SHOP) ---")
  
  ;; Xac dinh duong dan file Change-shape.exe
  (setq appPath "d:\\VIET\\00.INFORMATION\\00LISP\\cad\\My-tool\\Change-shape\\Change-shape.exe")
  (setq bridgePath "d:\\VIET\\00.INFORMATION\\00LISP\\cad\\My-tool\\Change-shape\\cad_bridge.json")
  
  (if (findfile appPath)
    (progn
      ;; Kiem tra xem nguoi dung co dang chon truoc MText nao khong
      (setq ss (ssget "_I" '((0 . "MTEXT,TEXT"))))
      (if (null ss)
        (progn
          (princ "\n[Mẹo] Ban co the chon truoc MText thanh thep hoac bam nut tren giao dien:")
          (setq ss (ssget ":S" '((0 . "MTEXT,TEXT"))))
        )
      )
      
      (if (and ss (> (sslength ss) 0))
        (progn
          (setq ent (ssname ss 0))
          (setq elist (entget ent))
          (setq text (cdr (assoc 1 elist)))
          (setq handle (cdr (assoc 5 elist)))
          
          ;; Ghi du lieu tam vao cad_bridge.json de Change-shape.exe doc tuc thi
          (setq f (open bridgePath "w"))
          (if f
            (progn
              (write-line (strcat "{\"handle\":\"" handle "\",\"text\":\"" (vl-string-translate "\\" "/" text) "\"}") f)
              (close f)
              (princ (strcat "\nDa ghi nhan thanh thep [Handle: " handle "]"))
            )
          )
        )
      )
      
      ;; Khoi dong Change-shape.exe
      (startapp appPath)
      (princ "\nDa mo cua so Change-shape.exe! Su dung giao dien de chon Shape va bam [Ap dung].")
    )
    (princ (strcat "\nKhong tim thay file: " appPath))
  )
  (princ)
)

(princ "\n[Change-shape] Da nap thanh cong! Go lenh 'CS' hoac 'CSH' de khoi dong.")
(princ)
