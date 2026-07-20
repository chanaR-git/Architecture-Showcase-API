# CONTROLLER_RULES

- **Validation:** Perform manual validation checks in controller methods; return BadRequest("message") for invalid input.
- **Response Style:** Use IActionResult with Ok(result) for success, BadRequest for validation errors, NotFound for missing resources.
- **Validation Library:** Do not use an external validation library; rely on manual checks and built-in ASP.NET attributes.
- **HTTP Status Codes:** Use standard codes: 200 (Ok), 400 (BadRequest), 404 (NotFound).
- **Authorization:** Apply [Authorize] attribute on protected endpoints.
- **Routing:** Use [Route("api/[controller]")] on controller classes.
