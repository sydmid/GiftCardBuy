// GiftCardBuy - Secure Code Reveal and Interactive UX
function revealCode(codeRecordId) {
    const el = document.getElementById(`code-val-${codeRecordId}`);
    if (!el) return;

    el.innerText = "در حال رمزگشایی امن...";

    fetch('/api/codes/reveal', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-CSRF-TOKEN': getCsrfToken()
        },
        body: JSON.stringify({ codeRecordId: codeRecordId })
    })
    .then(r => r.json())
    .then(data => {
        if (data.success) {
            el.innerText = data.code;
            if (data.pin) {
                el.innerText += ` (PIN: ${data.pin})`;
            }
        } else {
            el.innerText = "خطا در بازیابی کد";
            alert(data.message || "امکان نمایش کد وجود ندارد.");
        }
    })
    .catch(err => {
        el.innerText = "خطای ارتباطی";
    });
}

function copyCode(codeRecordId) {
    const el = document.getElementById(`code-val-${codeRecordId}`);
    if (!el) return;
    const text = el.innerText;
    if (text.includes("•••") || text.includes("رمزگشایی")) {
        alert("لطفاً ابتدا روی 'نمایش کد' کلیک کنید.");
        return;
    }
    navigator.clipboard.writeText(text).then(() => {
        alert("کد با موفقیت کپی شد! 📋");
    });
}

function getCsrfToken() {
    const match = document.cookie.match(/GiftCardBuy\.Antiforgery=([^;]+)/);
    return match ? match[1] : '';
}
