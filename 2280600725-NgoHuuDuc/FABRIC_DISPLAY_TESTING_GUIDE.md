# Fabric Display Testing Guide

## Quick Start

### Prerequisites
- Application running (`dotnet watch run`)
- Logged in as customer (not required for viewing)
- Products with assigned fabrics in database

---

## Test Scenario 1: Product Listing Page - Fabric Badges

### Steps
1. Navigate to **Sản phẩm** (Products) page
2. Look at product cards in the grid view
3. Scroll down to see fabric section

### Expected Results
✅ Each product card shows:
- Product name
- Product price
- **Fabric badges** (NEW)
- Stock status

✅ Fabric section displays:
- "Vải:" label
- Up to 3 fabric names as blue badges
- "+X khác" badge if more than 3 fabrics

### Example
```
Product: Elegant Suit
Price: 2,500,000 VNĐ
Vải: [Cotton] [Silk] [Linen] [+2 khác]
Status: Còn hàng
```

### Verification
- [ ] Fabric badges appear for products with fabrics
- [ ] Products without fabrics show no fabric section
- [ ] Badges are blue and readable
- [ ] "+X khác" badge appears correctly
- [ ] Badges wrap properly on mobile

---

## Test Scenario 2: Product Listing - Multiple Products

### Steps
1. Navigate to **Sản phẩm** page
2. View multiple products in the listing
3. Check different products have different fabrics

### Expected Results
✅ Each product shows its own fabrics
✅ Fabrics are different for different products
✅ No fabric duplication across products
✅ All products load without errors

---

## Test Scenario 3: Product Detail Page - Fabric Cards

### Steps
1. Click on any product to view details
2. Scroll down to find fabric section
3. Look for "Vải Có Sẵn" (Available Fabrics) section

### Expected Results
✅ Fabric section displays with heading
✅ Fabrics shown in grid layout (2 columns on desktop)
✅ Each fabric card shows:
- Fabric image (or placeholder)
- Fabric name
- Fabric group
- Composition
- Description
- Price

### Example Card
```
┌─────────────────────────┐
│   [Fabric Image]        │
│   Cotton                │
│   Nhóm: Natural         │
│   Thành phần: 100%      │
│   Mô tả: Soft and...    │
│   Giá: 50,000 VNĐ       │
└─────────────────────────┘
```

### Verification
- [ ] All fabric cards display correctly
- [ ] Images load properly
- [ ] Text is readable
- [ ] Cards have proper spacing
- [ ] Layout is responsive

---

## Test Scenario 4: Responsive Design - Mobile

### Steps
1. Open product listing on mobile device (or use browser DevTools)
2. Set viewport to mobile size (375px width)
3. Check fabric badges display
4. Navigate to product detail
5. Check fabric cards display

### Expected Results
✅ Product Listing:
- Fabric badges wrap properly
- Text remains readable
- No horizontal scrolling

✅ Product Detail:
- Fabric cards stack vertically (1 column)
- Images scale properly
- Text is readable
- No layout issues

### Verification
- [ ] No horizontal scrolling
- [ ] Text is readable
- [ ] Images scale properly
- [ ] Cards stack correctly
- [ ] Buttons are clickable

---

## Test Scenario 5: Responsive Design - Tablet

### Steps
1. Set viewport to tablet size (768px width)
2. Check product listing
3. Check product detail

### Expected Results
✅ Product Listing:
- Fabric badges display properly
- Grid layout works

✅ Product Detail:
- Fabric cards in 2-column layout
- Proper spacing
- Images display well

---

## Test Scenario 6: Product with No Fabrics

### Steps
1. Find a product with no assigned fabrics
2. View product detail page
3. Check if fabric section appears

### Expected Results
✅ No fabric section displayed
✅ No errors in console
✅ Page loads normally

---

## Test Scenario 7: Product with Many Fabrics

### Steps
1. Find a product with 5+ fabrics
2. View product listing
3. Check "+X khác" badge
4. View product detail
5. Check all fabrics display

### Expected Results
✅ Listing:
- Shows first 3 fabrics
- Shows "+X khác" badge

✅ Detail:
- Shows all fabrics
- Grid layout handles multiple cards
- No layout issues

---

## Test Scenario 8: Fabric Image Display

### Steps
1. View product detail with fabrics
2. Check if fabric images load
3. Check placeholder for missing images

### Expected Results
✅ Fabric images load correctly
✅ Missing images show placeholder icon
✅ Images have proper aspect ratio
✅ No broken image icons

---

## Test Scenario 9: Data Accuracy

### Steps
1. Note fabrics assigned in admin panel
2. View product in customer view
3. Compare displayed fabrics with admin assignment

### Expected Results
✅ Displayed fabrics match admin assignment
✅ No missing fabrics
✅ No extra fabrics
✅ Fabric order is consistent

### Verification Query
```sql
-- Check fabrics for product ID 1
SELECT f.Name, f.Composition, f.Price
FROM Fabrics f
INNER JOIN FabricProducts fp ON f.Id = fp.FabricId
WHERE fp.ProductId = 1
ORDER BY f.Name
```

---

## Test Scenario 10: Performance

### Steps
1. Open browser DevTools (F12)
2. Go to Network tab
3. Navigate to product listing
4. Check page load time
5. Navigate to product detail
6. Check page load time

### Expected Results
✅ Product listing loads in < 2 seconds
✅ Product detail loads in < 2 seconds
✅ No failed requests
✅ No console errors

### Performance Metrics
- Page Load Time: < 2 seconds
- Time to Interactive: < 3 seconds
- No 404 errors
- No console errors

---

## Test Scenario 11: Browser Compatibility

### Test on Multiple Browsers
- [ ] Chrome (latest)
- [ ] Firefox (latest)
- [ ] Safari (latest)
- [ ] Edge (latest)
- [ ] Mobile Chrome
- [ ] Mobile Safari

### Expected Results
✅ Fabric display works on all browsers
✅ Styling is consistent
✅ No layout issues
✅ Images load properly

---

## Test Scenario 12: Category Filtering

### Steps
1. Navigate to product listing
2. Select a category
3. Check fabric display for filtered products

### Expected Results
✅ Fabrics display correctly for filtered products
✅ Different categories show different fabrics
✅ No errors when filtering

---

## Troubleshooting

### Issue: Fabrics Not Showing
**Check:**
1. Are fabrics assigned in admin panel?
2. Are products visible (not hidden)?
3. Check browser console for errors
4. Check application logs

### Issue: Images Not Loading
**Check:**
1. Do images exist in wwwroot/images?
2. Are image URLs correct in database?
3. Check browser console for 404 errors
4. Check file permissions

### Issue: Layout Issues
**Check:**
1. Clear browser cache (Ctrl+Shift+Delete)
2. Hard refresh (Ctrl+Shift+R)
3. Check Bootstrap CSS is loaded
4. Check for CSS conflicts

### Issue: Performance Issues
**Check:**
1. Check application logs for errors
2. Monitor database queries
3. Check network tab in DevTools
4. Consider caching strategies

---

## Success Criteria

✅ All test scenarios pass
✅ Fabric display works on all pages
✅ Responsive design verified
✅ Performance acceptable
✅ No console errors
✅ Data accuracy confirmed
✅ Browser compatibility verified

---

## Sign-Off

- [ ] All tests passed
- [ ] No critical issues
- [ ] Ready for production
- [ ] User feedback positive

---

**Testing Date:** October 23, 2025
**Status:** Ready for Testing

