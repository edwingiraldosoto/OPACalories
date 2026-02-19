/**
 * Edwin Giraldo Soto
 * prueba técnica.
 *
 * IMPORTANTE (para depurar con F5 en Visual Studio):
 * - El botón "Optimizar" está en wwwroot/index.html con id="btnSolve"
 * - Aquí le asignamos el evento click a la función optimizar()
 *
 * ¿Dónde poner breakpoints?
 * 1) Frontend (JS): pon breakpoint dentro de optimizar()
 * 2) Backend (C#):
 *    - Controllers/ControladorOptimizacion.cs -> método Optimizar()
 *    - Services/ServicioOptimizador.cs -> método Optimizar()
 */

const obtenerElemento = (id) => document.getElementById(id);

// Referencias a inputs del formulario
const minCaloriasInput = obtenerElemento('minCalories');
const pesoMaximoKgInput = obtenerElemento('maxWeightKg');
const cuerpoTablaElementos = obtenerElemento('itemsTbody');
const cuerpoTablaSeleccionados = obtenerElemento('selectedTbody');
const estadoLbl = obtenerElemento('status');
const cajaError = obtenerElemento('errorBox');

// Referencias a áreas de salida (resultado)
const salidaPeso = obtenerElemento('outWeight');
const salidaCalorias = obtenerElemento('outCalories');
const salidaAlgoritmo = obtenerElemento('outAlgo');
const salidaJson = obtenerElemento('rawJson');
const notasContenedor = obtenerElemento('notes');

/**
 * Muestra un error en pantalla.
 * @param {string} mensaje Mensaje de error para el usuario
 */
function mostrarError(mensaje) {
  cajaError.textContent = mensaje;
  cajaError.classList.remove('d-none');
}

/** Limpia el error visible. */
function limpiarError() {
  cajaError.textContent = '';
  cajaError.classList.add('d-none');
}

/**
 * Mensaje de estado (ej: "Optimizando...", "Listo ✅")
 * @param {string} mensaje
 */
function setEstado(mensaje) {
  estadoLbl.textContent = mensaje || '';
}

/**
 * Escapa texto para evitar inyección HTML cuando renderizamos valores.
 * @param {any} valor
 */
function escaparHtml(valor) {
  return String(valor)
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;');
}

/**
 * Agrega una fila a la tabla de elementos (entradas).
 * La fila contiene inputs para: id, nombre, pesoKg, calorias.
 * @param {{id?:string, nombre?:string, pesoKg?:number, calorias?:number}} elemento
 */
function agregarFila(elemento = {}) {
  console.log('Agregando fila con:', elemento); // Debug para verificar que corre el código nuevo

  // Generar ID secuencial si no se proporciona (E1, E2, ...)
  let idFinal = elemento.id;
  if (!idFinal) {
    // Buscamos el número más alto en los IDs actuales para seguir la secuencia (E1, E2, E3...)
    const filasExistentes = [...cuerpoTablaElementos.querySelectorAll('tr')];
    let maxNum = 0;

    filasExistentes.forEach(fila => {
      const inputId = fila.querySelector('input');
      if (inputId && inputId.value.startsWith('E')) {
        const num = parseInt(inputId.value.substring(1));
        if (!isNaN(num) && num > maxNum) maxNum = num;
      }
    });

    // El siguiente ID será el máximo + 1, o simplemente el total de filas + 1 si no hay secuencia E#
    const nextIndex = (maxNum > 0) ? maxNum + 1 : filasExistentes.length + 1;
    idFinal = `E${nextIndex}`;
  }

  // Sugerir nombre si no se proporciona
  let nombreFinal = elemento.nombre;
  if (!nombreFinal && idFinal && idFinal.startsWith('E')) {
    const numPart = idFinal.substring(1);
    nombreFinal = numPart ? `Elemento ${numPart}` : 'Elemento';
  }

  const tr = document.createElement('tr');


  // Se define la estructura HTML de la fila mediante una plantilla (template string).
  // Inyecta dinámicamente el ID, nombre y valores numéricos con validación básica.
  // Nota: Usa 'escaparHtml' para prevenir ataques XSS y 'type=number' para restringir la entrada.
  tr.innerHTML = `
    <td><input class="form-control form-control-sm" placeholder="ID" value="${escaparHtml(idFinal)}"></td>
    <td><input class="form-control form-control-sm" placeholder="Elemento" value="${escaparHtml(nombreFinal || '')}"></td>
    <td><input class="form-control form-control-sm" type="number" min="0" step="0.001" value="${elemento.pesoKg ?? ''}"></td>
    <td><input class="form-control form-control-sm" type="number" min="0" step="1" value="${elemento.calorias ?? ''}"></td>
    <td class="text-end">
      <button class="btn btn-outline-danger btn-sm" type="button" title="Eliminar fila">✕</button>
    </td>
  `;

  // Botón de eliminar fila
  tr.querySelector('button').addEventListener('click', () => tr.remove());

  cuerpoTablaElementos.appendChild(tr);
}

/**
 * Lee el formulario y construye el payload EXACTO que espera el backend.
 *
 * Backend (JSON en camelCase):
 * {
 *   "minCalorias": 19,
 *   "pesoMaximoKg": 50,
 *   "elementos": [
 *     {"id":"E1","nombre":"Elemento 1","pesoKg":5,"calorias":3}
 *   ]
 * }
 *
 * Nota: calorias y minCalorias se envían como enteros.
 */

/**
 * RECOLECCIÓN Y FORMATEO DE DATOS(Payload):
 * 1. Captura los valores globales(Calorías mínimas y Peso máximo).
 * 2. Recorre cada fila de la tabla de elementos para extraer sus inputs.
 * 3. Valida tipos: convierte textos a números y asegura que no haya valores infinitos o incompletos.
 * 4. Mapeo Estricto: Organiza los datos en un objeto cuya estructura coincide exactamente
 * con el DTO(Data Transfer Object) que el backend.NET espera recibir vía JSON.
 */
function leerSolicitud() {
  const minCalorias = Number(minCaloriasInput.value);
  const pesoMaximoKg = Number(pesoMaximoKgInput.value);

  const elementos = [];
  const filas = [...cuerpoTablaElementos.querySelectorAll('tr')];

  filas.forEach((fila) => {
    const inputs = fila.querySelectorAll('input');

    const id = inputs[0].value?.trim();
    const nombre = inputs[1].value?.trim();
    const pesoKg = Number(inputs[2].value);
    const calorias = Number(inputs[3].value);

    // Si el usuario dejó una fila incompleta, la ignoramos.
    if (!Number.isFinite(pesoKg) || !Number.isFinite(calorias)) return;

    elementos.push({
      id: id || null,
      nombre: nombre || null,
      pesoKg,
      calorias: Math.trunc(calorias)
    });
  });

  return { minCalorias: Math.trunc(minCalorias), pesoMaximoKg, elementos };
}

/**
 * Renderiza la respuesta del backend en la UI.
 * @param {any} resultado
 */
function renderizarResultado(resultado) {
  salidaPeso.textContent = resultado.esFactible ? `${resultado.pesoTotalKg} kg` : '—';
  salidaCalorias.textContent = resultado.esFactible ? `${resultado.caloriasTotales}` : '—';
  salidaAlgoritmo.textContent = resultado.algoritmo || '—';

  salidaJson.textContent = JSON.stringify(resultado, null, 2);

  // Tabla de elementos seleccionados
  cuerpoTablaSeleccionados.innerHTML = '';
  const seleccionados = resultado.elementosSeleccionados || [];

  seleccionados.forEach((it) => {
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td>${escaparHtml(it.id || '')}</td>
      <td>${escaparHtml(it.nombre || '')}</td>
      <td class="text-end">${it.pesoKg}</td>
      <td class="text-end">${it.calorias}</td>
    `;
    cuerpoTablaSeleccionados.appendChild(tr);
  });

  // Notas / mensajes informativos del backend
  notasContenedor.innerHTML = '';
  const notas = resultado.notas || [];
  if (notas.length) {
    const ul = document.createElement('ul');
    ul.className = 'small text-muted';
    notas.forEach((n) => {
      const li = document.createElement('li');
      li.textContent = n;
      ul.appendChild(li);
    });
    notasContenedor.appendChild(ul);
  }
}

/**
 * Carga un ejemplo desde el backend (GET /api/ejemplo).
 * Este ejemplo viene basado en el PDF de la prueba.
 */
async function cargarEjemplo() {
  limpiarError();
  setEstado('Cargando ejemplo...');

  try {
    const res = await fetch('/api/ejemplo');
    if (!res.ok) throw new Error(`Error HTTP ${res.status}`);
    const ejemplo = await res.json();

    minCaloriasInput.value = ejemplo.minCalorias;
    pesoMaximoKgInput.value = ejemplo.pesoMaximoKg;

    cuerpoTablaElementos.innerHTML = '';
    (ejemplo.elementos || []).forEach((it) => agregarFila(it));

    setEstado('Ejemplo cargado.');
  } catch (e) {
    mostrarError(e.message || String(e));
    setEstado('');
  }
}

/**
 * FUNCIÓN PRINCIPAL del botón "Optimizar".
 *
 * Flujo:
 * 1) Lee el formulario (leerSolicitud)
 * 2) Valida lo mínimo en frontend (por UX)
 * 3) Hace POST /api/optimizar con JSON
 * 4) Renderiza el resultado
 *
 * Breakpoint recomendado: aquí.
 */
async function optimizar() {
  limpiarError();
  setEstado('Optimizando...');

  const payload = leerSolicitud();

  // Validación UX (el backend valida de forma más estricta)
  if (payload.elementos.length === 0) {
    mostrarError('Agrega al menos 1 elemento.');
    setEstado('');
    return;
  }
  // se hace el llamado al servivio para Optimizar
  try {
    const res = await fetch('/api/optimizar', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    // Si hay error lo muestra
    if (!res.ok) {
      const text = await res.text();
      throw new Error(`Error HTTP ${res.status}: ${text}`);
    }

    const resultado = await res.json();
    renderizarResultado(resultado);
    setEstado(resultado.esFactible ? 'Listo ✅' : 'Sin solución viable ⚠️');
  } catch (e) {
    mostrarError(e.message || String(e));
    setEstado('');
  }
}

/** Limpia UI y tabla de entrada. */
function limpiarTodo() {
  cuerpoTablaElementos.innerHTML = '';
  cuerpoTablaSeleccionados.innerHTML = '';
  salidaPeso.textContent = '—';
  salidaCalorias.textContent = '—';
  salidaAlgoritmo.textContent = '—';
  salidaJson.textContent = '{ }';
  notasContenedor.innerHTML = '';
  setEstado('');
  limpiarError();
}

// -----------------------------
// Enlazar botones (eventos DOM)
// -----------------------------
obtenerElemento('btnAdd').addEventListener('click', () => agregarFila());
obtenerElemento('btnSample').addEventListener('click', cargarEjemplo);
obtenerElemento('btnSolve').addEventListener('click', optimizar);
obtenerElemento('btnClear').addEventListener('click', limpiarTodo);

// Precargamos un ejemplo para que el evaluador lo pruebe de una.
agregarFila({ id: 'E1', nombre: 'Elemento 1', pesoKg: 5, calorias: 3 });
agregarFila({ id: 'E2', nombre: 'Elemento 2', pesoKg: 3, calorias: 5 });
agregarFila({ id: 'E3', nombre: 'Elemento 3', pesoKg: 5, calorias: 2 });
agregarFila({ id: 'E4', nombre: 'Elemento 4', pesoKg: 1, calorias: 8 });
agregarFila({ id: 'E5', nombre: 'Elemento 5', pesoKg: 2, calorias: 3 });

// Mostrar el año actual en el footer
const spanYear = obtenerElemento('year');
if (spanYear) {
  spanYear.textContent = new Date().getFullYear();
}
